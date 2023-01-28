using System.Text;

namespace Containerizr;

public class DockerfileContext
{
    private StringBuilder content;
    private readonly ContainerImage image;
    private readonly string contextDirectoryPath;
    private readonly string? subPath;

    private DockerfileContext(ContainerImage image, string contextDirectoryPath)
    {
        this.image = image;
        this.contextDirectoryPath = contextDirectoryPath;
        content = new StringBuilder($"FROM {image.BaseImage}\r\n\r\n");
    }
    private DockerfileContext(ContainerImage image, string contextDirectoryPath, string name)
    {
        this.image = image;
        this.contextDirectoryPath = contextDirectoryPath;
        this.subPath = $"_{name}";
        content = new StringBuilder($"FROM {image.BaseImage} AS {name}\r\n\r\n");
    }

    public async Task AddMultiStageImage(ContainerImage image, string fromName)
    {
        var ctx = CloneForMultiStageBuildImage(image, fromName);

        var resp = await this.image.CreateDockerContext(ctx);

        if (!resp.IsSuccess)
        {
            throw new Exception(resp.Exception!.GetBaseException().Message);
        }

        this.content = new StringBuilder($"{resp.Content}\r\n\r\n{content}");
    }

    public void AddDirective(string content)
    {
        this.content.AppendLine(content);
    }

    private DockerfileContext CloneForMultiStageBuildImage(ContainerImage image, string fromName)
        => new DockerfileContext(image, contextDirectoryPath, fromName);

    public static DockerfileContext Create(ContainerImage image, string contextDirectoryPath) 
        => new DockerfileContext(image, contextDirectoryPath);

    public string GetContent() => content.ToString();

    public ContainerImage Image => image;
    public string ContextRootRelativePath => subPath ?? ".";
    public string ContextDirectoryPath => $"{contextDirectoryPath}{(subPath != null ? $"/{subPath}" : "")}";
}
