namespace Containerizr.Directives;

public class SetCommandDirective : DockerDirective
{
    private readonly string cmd;

    public SetCommandDirective(string cmd)
    {
        this.cmd = cmd;
    }

    public override Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context)
        => Task.FromResult(DockerDirectiveResponse.Empty);

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"CMD {cmd}");
        return Task.CompletedTask;
    }
}
