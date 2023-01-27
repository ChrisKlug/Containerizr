using System.Text;
using System.Xml.Linq;

namespace Containerizr;

public class DockerfileContext
{
    private StringBuilder content;

    private DockerfileContext(string baseImage, string workingDirectory, string contextDirectory)
    {
        content = new StringBuilder($"FROM {baseImage}\r\n\r\n");
        WorkingDirectory = workingDirectory;
        ContextDirectory = contextDirectory;
    }
    private DockerfileContext(string baseImage, string workingDirectory, string contextDirectory, string name)
    {
        content = new StringBuilder($"FROM {baseImage} AS {name}\r\n\r\n");
        WorkingDirectory = workingDirectory;
        ContextDirectory = contextDirectory;
        RootRelativePath = "./" + Path.GetFileName(contextDirectory); // Weird, but gets directory name as well
    }

    public async Task AddMultiStageImage(ContainerImage image, string fromName)
    {
        var resp = await image.GenerateMultiStageDockerContext(ContextDirectory, fromName);

        if (!resp.IsSuccess)
        {
            throw new Exception(resp.Exception.GetBaseException().Message);
        }

        this.content = new StringBuilder($"{resp.Content}\r\n\r\n{content}");
    }

    public void AddDirective(string content)
    {
        this.content.AppendLine(content);
    }

    public static DockerfileContext Create(string baseImage, string workingDirectory, string contextDirectory)
        => new DockerfileContext(baseImage, workingDirectory, contextDirectory);
    public static DockerfileContext CreateForMultiStageImage(string baseImage, string workingDirectory, string contextDirectory, string name)
        => new DockerfileContext(baseImage, workingDirectory, contextDirectory, name);


    public string GetContent() => content.ToString();

    public string ContextDirectory { get; }
    public string WorkingDirectory { get; set; }
    public string RootRelativePath { get; private set; } = ".";
}
