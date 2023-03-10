namespace Containerizr.Directives;

public class AddFilesDirective : DockerDirective
{
    private readonly string[] sources;
    private readonly string target;
    private readonly string? addName = null;

    public AddFilesDirective(string[] sources, string target)
    {
        this.sources = sources;
        this.target = target;
    }
    public AddFilesDirective(string[] sources, string target, string? addName)
        : this(sources, target)
    {
        this.addName = addName;
    }

    public override async Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
    {
        if (sources.Any(x => !File.Exists(x)))
        {
            throw new Exception("File does not exist");
        }

        foreach (var source in sources)
        {
            await context.Image.InteractiveContainer.ExecuteDockerCommand($"cp \"{source}\" {context.Image.InteractiveContainer.Name}:{target}");
        }

        return CommandExecutionResponse.Create("", "");
    }

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        var filesDirectory = addName ?? Guid.NewGuid().ToString();
        var dirPath = Path.Combine(context.ContextDirectoryPath, filesDirectory);
        Directory.CreateDirectory(dirPath);

        foreach (var source in sources)
        {
            var fileName = Path.GetFileName(source);
            var copyTarget = Path.Combine(dirPath, fileName);
            File.Copy(source, copyTarget);
        }

        var filename = sources.Length == 1 ? Path.GetFileName(sources[0]) : "";

        context.AddDirective($"COPY {context.ContextRootRelativePath}/{filesDirectory}/{filename} {target}");

        return Task.CompletedTask;
    }
}
