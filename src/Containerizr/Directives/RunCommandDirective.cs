namespace Containerizr.Directives;

public class RunCommandDirective : DockerDirective
{
    private readonly string command;

    public RunCommandDirective(string command)
    {
        this.command = command;
    }

    public override Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
        => context.Image.ExecuteCommand(command);

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"RUN {this.command}");
        return Task.CompletedTask;
    }
}