namespace Containerizr.Directives;

public class SetEntryPointDirective : DockerDirective
{
    private readonly string entrypoint;

    public SetEntryPointDirective(string entrypoint)
    {
        this.entrypoint = entrypoint;
    }

    public override Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context)
        => Task.FromResult(DockerDirectiveResponse.Empty);

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"ENTRYPOINT {entrypoint}");
        return Task.CompletedTask;
    }
}
