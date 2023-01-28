namespace Containerizr.Directives;

public class AddDirectoryDirective : DockerDirective
{
    private readonly string directory;
    private readonly string target;
    private readonly string? addName = null;

    public AddDirectoryDirective(string directory, string target)
    {
        this.directory = directory;
        this.target = target;

        if (!this.directory.EndsWith("/"))
        {
            this.directory += "/";
        }
    }
    public AddDirectoryDirective(string directory, string target, string? addName)
        : this(directory, target)
    {
        this.addName = addName;
    }

    public override async Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
    {
        if (!Directory.Exists(directory))
        {
            throw new Exception("Directory does not exist");
        }

        await context.Image.InteractiveContainer.ExecuteDockerCommand($"cp \"{directory}.\" {context.Image.InteractiveContainer.Name}:{target}");

        return CommandExecutionResponse.Create("", "");
    }

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        string filesDirectory;
        if (addName != null)
        {
            filesDirectory = "ctx_" + addName;
        }
        else
        {
            filesDirectory = Guid.NewGuid().ToString();
        }
        var dirPath = Path.Combine(context.ContextDirectoryPath, filesDirectory);
        Directory.CreateDirectory(dirPath);

        CopyFilesRecursively(directory, dirPath);

        context.AddDirective($"COPY {context.ContextRootRelativePath}/{filesDirectory} {target}");

        return Task.CompletedTask;
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        targetPath += "/";

        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(filePath, filePath.Replace(sourcePath, targetPath), true);
        }
    }
}
