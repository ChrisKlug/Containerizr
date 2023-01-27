using Containerizr.Directives;
using System.Diagnostics;

namespace Containerizr;

public class ContainerImage : IDisposable, IHaveWorkingDir
{
    private readonly ContainerImageConfig config;
    private readonly List<DockerDirective> directives = new List<DockerDirective>();
    private string workingDir;
    private bool disposedValue;
    private Process? interactiveContainer;

    protected ContainerImage(ContainerImageConfig config, string workingDir)
	{
        this.config = config;
        this.workingDir = workingDir;
    }

    public async Task<DockerDirectiveResponse> AddDirective(DockerDirective directive)
    {
        directives.Add(directive);
        if (!config.IsInteractive)
        {
            return DockerDirectiveResponse.NonInteractive;
        }

        await EnsureInteractiveContainerIsRunning();

        return await directive.ExecuteInteractive(ExecutionContext.Create(config, workingDir));
    }
    public async Task<DockerfileContentGenerationResponse> GetDockerFileContent(string? tempDirectoryPath = null)
    {
        var dir = Path.Combine(tempDirectoryPath ?? Path.Combine(Path.GetTempPath(), $"{config.ContainerName}_context"));
        Directory.CreateDirectory(dir);

        var response = await CreateDockerContext(dir);

        Directory.Delete(dir, true);

        return response;
    }
    public async Task<DockerfileContentGenerationResponse> GenerateMultiStageDockerContext(string contextDirectory, string imageName)
    {
        var dir = Path.Combine(contextDirectory, "_" + imageName);
        Directory.CreateDirectory(dir);

        var ctx = DockerfileContext.CreateForMultiStageImage(config.BaseImage, workingDir, dir, imageName);
        try
        {
            foreach (var directive in directives)
            {
                await directive.GenerateDockerFileContent(ctx);
            }

            var content = ctx.GetContent();

            return DockerfileContentGenerationResponse.Create(content);
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

    private async Task<DockerfileContentGenerationResponse> CreateDockerContext(string contextPath)
    {
        var ctx = DockerfileContext.Create(config.BaseImage, workingDir, contextPath);
        try
        {
            foreach (var directive in directives)
            {
                await directive.GenerateDockerFileContent(ctx);
            }

            var content = ctx.GetContent();

            File.WriteAllText(Path.Combine(contextPath, "dockerfile"), content);

            return DockerfileContentGenerationResponse.Create(content);
        }
        catch (Exception ex)
        {
            return DockerfileContentGenerationResponse.Create(ex);
        }
    }

    string IHaveWorkingDir.WorkingDir
    {
        get => workingDir;
        set => workingDir = value;
    }
    public string ContainerName => config.ContainerName;

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
