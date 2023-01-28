using System.Text;

namespace Containerizr;

public class DockerfileContext
{
    private StringBuilder content;
    private readonly ContainerImage image;

    private DockerfileContext(ContainerImage image)
    {
        this.image = image;
        content = new StringBuilder($"FROM {image.BaseImage}\r\n\r\n");
    }
    private DockerfileContext(ContainerImage image, string name)
    {
        this.image = image;
        content = new StringBuilder($"FROM {image.BaseImage} AS {name}\r\n\r\n");
    }

    public async Task AddMultiStageImage(ContainerImage image, string fromName)
    {
        var resp = await this.image.CreateMultiStageDockerContext(image, fromName);

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

    public static DockerfileContext Create(ContainerImage image) => new DockerfileContext(image);
    public static DockerfileContext CreateForMultiStageImage(ContainerImage image, string name) => new DockerfileContext(image, name);


    public string GetContent() => content.ToString();

    public string WorkingDirectory 
    {
        get => image.Items.GetWorkingDirectory();
        set
        {
            image.Items.SetItem("BuiltIn.WorkingDirectory", value);
        }
    }
    public string ContextDirectoryPath => image.Items.GetContextDirectory();
    public string RootRelativePath => image.Items.GetItem<string>("BuiltIn.RootRelativeContextPath") ?? "";
}
