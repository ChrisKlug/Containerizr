using Containerizr.Directives;
using System.Diagnostics;

namespace Containerizr;

public class ContainerImage : IDisposable
{
    private readonly ContainerImageConfig config;
    private readonly List<DockerDirective> directives = new List<DockerDirective>();
    private bool disposedValue;
    private Process? interactiveContainer;

    protected ContainerImage(ContainerImageConfig config, string workingDir)
	{
        this.config = config;
        
        State = new ContainerImageState();
        State.SetItem("BuiltIn.WorkingDir", workingDir);
    }

    public async Task<DockerDirectiveResponse> AddDirective(DockerDirective directive)
    {
        directives.Add(directive);
        if (!config.IsInteractive)
        {
            return DockerDirectiveResponse.NonInteractive;
        }

        await EnsureInteractiveContainerIsRunning();

        return await directive.ExecuteInteractive(ExecutionContext.Create(config, State.GetWorkingDirectory()));
    }
    public async Task<DockerfileContentGenerationResponse> CreateDockerContext(string? tempDirectoryPath = null, bool doNotDelete = false)
    {
        var dir = Path.Combine(tempDirectoryPath ?? Path.Combine(Path.GetTempPath(), $"{config.ContainerName}_context"));
        Directory.CreateDirectory(dir);

        State.SetItem("BuiltIn.RootContextDir", dir);
        State.SetItem("BuiltIn.ContextDir", dir);
        State.SetItem("BuiltIn.RootRelativeContextPath", "");

        var ctx = DockerfileContext.Create(this);
        string content;
        try
        {
            foreach (var directive in directives)
            {
                await directive.GenerateDockerFileContent(ctx);
            }

            content = ctx.GetContent();

            File.WriteAllText(Path.Combine(dir, "dockerfile"), content);
        }
        catch (Exception ex)
        {
            return DockerfileContentGenerationResponse.Create(ex);
        }

        if (!doNotDelete)
            Directory.Delete(dir, true);

        return DockerfileContentGenerationResponse.Success(content);
    }
    public async Task<DockerfileContentGenerationResponse> GenerateMultiStageDockerContext(ContainerImage image, string imageName)
    {
        var rootContextDir = State.GetItem<string>("BuiltIn.RootContextDir")!;
        var contentDir = $"_{imageName}";
        var dir = Path.Combine(Path.Combine(rootContextDir, contentDir));
        Directory.CreateDirectory(dir);

        image.State.SetItem("BuiltIn.RootContextDir", rootContextDir);
        image.State.SetItem("BuiltIn.ContextDir", dir);
        image.State.SetItem("BuiltIn.RootRelativeContextPath", contentDir);

        var ctx = DockerfileContext.CreateForMultiStageImage(image, imageName);
        try
        {
            foreach (var directive in image.directives)
            {
                await directive.GenerateDockerFileContent(ctx);
            }

            var content = ctx.GetContent();

            return DockerfileContentGenerationResponse.Success(content);
        }
        catch (Exception ex)
        {
            return DockerfileContentGenerationResponse.Create(ex);
        }
    }
    public async Task CreateImage(string name, string? tag = null, string? tempDirectoryPath = null)
    {
        var dir = Path.Combine(tempDirectoryPath ?? Path.GetTempPath(), $"{config.ContainerName}_context");
        Directory.CreateDirectory(dir);

        var response = await CreateDockerContext(dir);

        var si = new ProcessStartInfo("docker", $"build -t {name}:{(tag != null ? tag : "latest")} .")
        {
            WorkingDirectory = dir,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var process = Process.Start(si)!;

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
                Console.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
                Console.Error.WriteLine(e.Data);
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        Console.Error.WriteLine();

        Directory.Delete(dir, true);
    }

    protected virtual Process SetUpInteractiveContainer(ContainerImageConfig config)
    {
        var si = new ProcessStartInfo("docker", $"run -i --rm --name {config.ContainerName} {config.BaseImage}")
        {
            CreateNoWindow = true,
        };
        return Process.Start(si)!;
    }
    protected async Task EnsureInteractiveContainerIsRunning()
    {
        if (config.IsInteractive)
        {
            if (interactiveContainer == null)
            {
                interactiveContainer = SetUpInteractiveContainer(config);

                var si = new ProcessStartInfo("docker", "container inspect -f '{{.State.Running}}' " + config.ContainerName)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                var isRunning = false;
                Process process;
                while (!isRunning)
                {
                    process = Process.Start(si)!;
                    await process.WaitForExitAsync();
                    var response = await process.StandardOutput.ReadLineAsync();
                    isRunning = response == "'true'";
                }
            }
        }
    }

    public string ContainerName => config.ContainerName;
    public string BaseImage => config.BaseImage;
    public ContainerImageState State { get; private set; }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (interactiveContainer != null)
                {
                    interactiveContainer.Close();

                    var si = new ProcessStartInfo("docker", $"stop {config.ContainerName}")
                    {
                        CreateNoWindow = true,
                    };
                    Process.Start(si)!.WaitForExit();
                }
            }

            disposedValue = true;
        }
    }

    ~ContainerImage()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
