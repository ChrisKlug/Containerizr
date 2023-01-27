using System.Diagnostics;

namespace Containerizr.Directives;

public abstract class DockerDirective
{
    public abstract Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context);

    public abstract Task GenerateDockerFileContent(DockerfileContext context);

    protected async Task<DockerDirectiveResponse> ExecuteDockerCommand(string command)
    {
        var si = new ProcessStartInfo("docker", $"{command}")
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var process = Process.Start(si)!;

        await process.WaitForExitAsync();

        var ret = await DockerDirectiveResponse.Create(process);

        return ret;
    }
}

public abstract class LinuxDockerDirective : DockerDirective
{
    protected Task<DockerDirectiveResponse> ExecuteCommandInContainer(ExecutionContext context, string command)
        => ExecuteDockerCommand($"exec {context.ContainerConfig.ContainerName} bash -c \"cd {context.WorkingDirectory} && {command.Replace("\"", "\\\"")}\"");
}
