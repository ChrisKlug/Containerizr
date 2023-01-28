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

    protected ContainerImage(string baseImage, string initialWorkingDirectory, bool? interactive)
	{   
        Items = new ContainerImageItems();
        BaseImage = baseImage;
        InteractiveContainer = new InteractiveContainer(this, containerName, initialWorkingDirectory, GetCommandFormatter(), interactive ?? InteractiveDefault);
        Count++;
    }

    public async Task<CommandExecutionResponse> AddDirective(DockerDirective directive)
    {
        directives.Add(directive);

        await InteractiveContainer.Initialize();

        return await directive.ExecuteInteractive(InteractiveContainer.Context);
    }
    
    public async Task<DockerfileContentGenerationResponse> CreateDockerContext(string? tempDirectoryPath = null)
    {
        var dir = Path.Combine(tempDirectoryPath ?? Path.Combine(Path.GetTempPath(), $"{containerName}_context"));

        var ctx = DockerfileContext.Create(this, dir);

        var response = await CreateDockerContext(ctx);
        if (!response.IsSuccess)
        {
            if (tempDirectoryPath == null)
            {
                Directory.Delete(dir, true);
            }
            throw response.Exception!;
        }

        File.WriteAllText(Path.Combine(dir, "dockerfile"), response.Content);

        return response;
    }
    internal async Task<DockerfileContentGenerationResponse> CreateDockerContext(DockerfileContext context)
    {
        Directory.CreateDirectory(context.ContextDirectoryPath);

        try
        {
            foreach (var directive in context.Image.directives)
            {
                await directive.GenerateDockerFileContent(context);
            }

            var content = context.GetContent();

            return DockerfileContentGenerationResponse.Success(content, context.ContextDirectoryPath);
        }
        catch (Exception ex)
        {
            return DockerfileContentGenerationResponse.Create(ex, context.ContextDirectoryPath);
        }
    }
    public async Task CreateImage(string name, string? tag = null, string? tempDirectoryPath = null)
    {
        var dir = Path.Combine(tempDirectoryPath ?? Path.GetTempPath(), $"{containerName}_context");

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
    private Func<(string Command, string WorkingDirectory), string> GetCommandFormatter()
        => x => FormatCommand(x.Command, x.WorkingDirectory);

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
