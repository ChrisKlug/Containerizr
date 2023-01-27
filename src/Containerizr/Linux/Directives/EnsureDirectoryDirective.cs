using Containerizr.Directives;

namespace Containerizr.Linux.Directives;

public class EnsureDirectoryDirective : LinuxDockerDirective
{
    private readonly string path;

    public EnsureDirectoryDirective(string path)
    {
        this.path = path;
    }

    public override Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context)
        => ExecuteCommandInContainer(context, $"mkdir -p {path}");

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"RUN mkdir -p {path}");

        return Task.CompletedTask;
    }
}
