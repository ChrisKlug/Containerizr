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

    public override async Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var response = await ExecuteDockerCommand($"cp {image.ContainerName}:{source} {tempDir}");
        if (response.HasError)
        {
            return response;
        }

        var target = Path.GetFileName(source);
        var suffix = target.IndexOf(".") > 0 ? "" : "/.";
        response = await ExecuteDockerCommand($"cp {tempDir}/{target}{suffix} {context.ContainerConfig.ContainerName}:{this.target} ");
        if (response.HasError)
        {
            return response;
        }
        Directory.Delete(tempDir, true);
        return DockerDirectiveResponse.Empty;
    }

    public override async Task GenerateDockerFileContent(DockerfileContext context)
    {
        var fromName = this.fromName ?? image.ContainerName;
        await context.AddMultiStageImage(image, fromName);
        context.AddDirective($"COPY --from={fromName} {source} {target}");
    }
}
