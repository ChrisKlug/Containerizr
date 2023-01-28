using Containerizr.Directives;

namespace Containerizr.Linux.Directives;

public class EnsureDirectoryDirective : LinuxDockerDirective
{
    private readonly string path;

    public EnsureDirectoryDirective(string path)
    {
        this.path = path;
    }

    public override Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
        => context.Image.ExecuteCommand($"mkdir -p {path}");

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"RUN mkdir -p {path}");

        return Task.CompletedTask;
    }
}
