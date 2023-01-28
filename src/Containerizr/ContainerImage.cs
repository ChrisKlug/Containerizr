using Containerizr.Directives;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace Containerizr;

public abstract class ContainerImage : IDisposable
{
    private static int Count = 0;
#if DEBUG
    private const bool InteractiveDefault = true;
#else
    private const bool InteractiveDefault = false;
#endif
    private readonly List<DockerDirective> directives = new List<DockerDirective>();
    private Process? interactiveContainer;
    private bool isDisposed;

    protected ContainerImage(string baseImage, string workingDir, bool? interactive)
	{   
        Items = new ContainerImageItems();
        Items.SetItem("BuiltIn.WorkingDir", workingDir);
        BaseImage = baseImage;
        IsInteractive = interactive ?? InteractiveDefault;
        Count++;
    }

    public async Task<CommandExecutionResponse> AddDirective(DockerDirective directive)
    {
        directives.Add(directive);
        if (!IsInteractive)
        {
            return CommandExecutionResponse.NonInteractive;
        }

        await EnsureInteractiveContainerIsRunning();

        return await directive.ExecuteInteractive(ExecutionContext.Create(this, Items.GetWorkingDirectory()));
    }
    
    public async Task<DockerfileContentGenerationResponse> CreateDockerContext(string? tempDirectoryPath = null, bool doNotDelete = false)
    {
        var dir = Path.Combine(tempDirectoryPath ?? Path.Combine(Path.GetTempPath(), $"{InteractiveContainerName}_context"));
        Directory.CreateDirectory(dir);

        Items.SetItem("BuiltIn.RootContextDir", dir);
        Items.SetItem("BuiltIn.ContextDir", dir);
        Items.SetItem("BuiltIn.RootRelativeContextPath", "");

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
    public async Task<DockerfileContentGenerationResponse> CreateMultiStageDockerContext(ContainerImage image, string imageName)
    {
        var rootContextDir = Items.GetItem<string>("BuiltIn.RootContextDir")!;
        var contentDir = $"_{imageName}";
        var dir = Path.Combine(Path.Combine(rootContextDir, contentDir));
        Directory.CreateDirectory(dir);

        image.Items.SetItem("BuiltIn.RootContextDir", rootContextDir);
        image.Items.SetItem("BuiltIn.ContextDir", dir);
        image.Items.SetItem("BuiltIn.RootRelativeContextPath", contentDir);

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
        var dir = Path.Combine(tempDirectoryPath ?? Path.GetTempPath(), $"{InteractiveContainerName}_context");
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

    protected internal Task<CommandExecutionResponse> ExecuteCommand(string command)
    {
        return ExecuteDockerCommand($"exec {InteractiveContainerName} {FormatCommand(Items.GetWorkingDirectory(), command)}");
    }
    protected internal async Task<CommandExecutionResponse> ExecuteDockerCommand(string command)
    {
        var si = new ProcessStartInfo("docker", command)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var process = Process.Start(si)!;

        await process.WaitForExitAsync();

        return await CommandExecutionResponse.Create(process);
    }
    protected virtual Process SetUpInteractiveContainer()
    {
        var si = new ProcessStartInfo("docker", $"run -i --rm --name {InteractiveContainerName} {BaseImage}")
        {
            CreateNoWindow = true,
        };
        return Process.Start(si)!;
    }
    protected async Task EnsureInteractiveContainerIsRunning()
    {
        if (IsInteractive)
        {
            if (interactiveContainer == null)
            {
                interactiveContainer = SetUpInteractiveContainer();

                var si = new ProcessStartInfo("docker", "container inspect -f '{{.State.Running}}' " + InteractiveContainerName)
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

    protected abstract string FormatCommand(string command, string currentDirectory);

    public ContainerImageItems Items { get; private set; }
    public string InteractiveContainerName { get; init; } = $"containerizr_{DateTime.Now.ToString("HHmmss")}_{Count}";
    public bool IsInteractive { get; }
    public string BaseImage { get; }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                if (interactiveContainer != null)
                {
                    interactiveContainer.Close();

                    var si = new ProcessStartInfo("docker", $"stop {InteractiveContainerName}")
                    {
                        CreateNoWindow = true,
                    };
                    Process.Start(si)!.WaitForExit();
                }
            }

            isDisposed = true;
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
