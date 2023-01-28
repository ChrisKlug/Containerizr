namespace Containerizr.Directives;

public class CopyFromImageDirective : DockerDirective
{
    private readonly ContainerImage image;
    private readonly string source;
    private readonly string target;
    private readonly string? fromName = null;

    public CopyFromImageDirective(ContainerImage image, string source, string target)
    {
        this.image = image;
        this.source = source;
        this.target = target;
    }
    public CopyFromImageDirective(ContainerImage image, string source, string target, string? fromName)
        : this(image, source, target)
    {
        this.fromName = fromName;
    }

    public override async Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var response = await context.Image.ExecuteDockerCommand($"cp {image.InteractiveContainerName}:{source} {tempDir}");
        if (response.HasError)
        {
            return response;
        }

        var target = Path.GetFileName(source);
        var suffix = target.IndexOf(".") > 0 ? "" : "/.";
        response = await context.Image.ExecuteDockerCommand($"cp {tempDir}/{target}{suffix} {context.Image.InteractiveContainerName}:{this.target} ");
        if (response.HasError)
        {
            return response;
        }
        Directory.Delete(tempDir, true);
        return CommandExecutionResponse.Empty;
    }

    public override async Task GenerateDockerFileContent(DockerfileContext context)
    {
        var fromName = this.fromName ?? image.InteractiveContainerName;
        await context.AddMultiStageImage(image, fromName);
        context.AddDirective($"COPY --from={fromName} {source} {target}");
    }
}
