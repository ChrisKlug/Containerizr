namespace Containerizr.Directives;

public abstract class DockerDirective
{
    public abstract Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context);

    public abstract Task GenerateDockerFileContent(DockerfileContext context);
}

public abstract class LinuxDockerDirective : DockerDirective
{
}
