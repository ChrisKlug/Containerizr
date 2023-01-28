using Containerizr.Directives;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace Containerizr;

public abstract class ContainerImage : IDisposable
{
#if DEBUG
    private const bool InteractiveDefault = true;
#else
    private const bool InteractiveDefault = false;
#endif
    private readonly List<DockerDirective> directives = new List<DockerDirective>();
    private static int Count = 0;
    private string containerName = $"containerizr_{DateTime.Now.ToString("HHmmss")}_{Count}";
    private bool isDisposed;

    protected ContainerImage(string baseImage, string workingDir, bool? interactive)
	{   
        Items = new ContainerImageItems();
        Items.SetItem("BuiltIn.WorkingDir", workingDir);
        BaseImage = baseImage;
        InteractiveContainer = new InteractiveContainer(this, containerName, interactive ?? InteractiveDefault);
        Count++;
    }

    public async Task<CommandExecutionResponse> AddDirective(DockerDirective directive)
    {
        directives.Add(directive);

        await InteractiveContainer.Initialize();

        return await directive.ExecuteInteractive(ExecutionContext.Create(this, Items.GetWorkingDirectory()));
    }
    
    public async Task<DockerfileContentGenerationResponse> CreateDockerContext(string? tempDirectoryPath = null, bool doNotDelete = false)
    {
        var dir = Path.Combine(tempDirectoryPath ?? Path.Combine(Path.GetTempPath(), $"{containerName}_context"));
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
        var dir = Path.Combine(tempDirectoryPath ?? Path.GetTempPath(), $"{containerName}_context");
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

    protected internal abstract string FormatCommand(string command, string currentDirectory);

    public ContainerImageItems Items { get; private set; }
    public string BaseImage { get; }
    public InteractiveContainer InteractiveContainer { get; private set; }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                if (InteractiveContainer != null)
                {
                    InteractiveContainer.Dispose();
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
