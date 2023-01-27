using Containerizr.Directives;

namespace Containerizr.Linux.Directives;

public class SetEnvironmentVariableDirective : LinuxDockerDirective
{
    private readonly string name;
    private readonly string value;

    public SetEnvironmentVariableDirective(string name, string value)
    {
        this.name = name;
        this.value = value;
    }

    public override Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context)
        => ExecuteCommandInContainer(context, $"echo \"export {name}='{value}'\" >> /etc/profile && source /etc/profile");

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"ENV {name}=\"{value}\"");

        return Task.CompletedTask;
    }
}
