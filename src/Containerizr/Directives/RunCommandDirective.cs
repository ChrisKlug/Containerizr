namespace Containerizr.Directives;

public class RunCommandDirective : DockerDirective
{
    private readonly Func<CommandContext, string> commandCallback;

    public RunCommandDirective(Func<CommandContext, string> commandCallback)
    {
        this.commandCallback = commandCallback;
    }

    public override Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context)
        => ExecuteDockerCommand($"exec {context.ContainerConfig.ContainerName} {commandCallback(CommandContext.Create(context))}");

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"RUN {commandCallback(CommandContext.Create(context))}");
        return Task.CompletedTask;
    }

    public class CommandContext
    {
        private CommandContext(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }

        public static CommandContext Create(ExecutionContext ctx)
            => new CommandContext(ctx.WorkingDirectory);
        public static CommandContext Create(DockerfileContext ctx)
            => new CommandContext(ctx.WorkingDirectory);

        public string WorkingDirectory { get; }
    }
}