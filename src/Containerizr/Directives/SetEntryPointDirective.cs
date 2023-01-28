namespace Containerizr.Directives;

public class SetEntryPointDirective : DockerDirective
{
    private readonly string entrypoint;

    public SetEntryPointDirective(string entrypoint)
    {
        this.entrypoint = entrypoint;
    }

    public override Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
        => Task.FromResult(CommandExecutionResponse.Empty);

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"ENTRYPOINT {entrypoint}");
        return Task.CompletedTask;
    }
}
