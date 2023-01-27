namespace Containerizr.Directives;

public class SetWorkingDirDirective : DockerDirective
{
    private readonly string dir;

    public SetWorkingDirDirective(string dir)
    {
        this.dir = dir;
    }

    public override async Task<DockerDirectiveResponse> ExecuteInteractive(ExecutionContext context)
    {
        var resp = await ExecuteDockerCommand($"exec {context.ContainerConfig.ContainerName} cd {dir}");
        if (!resp.HasError)
        {
            context.WorkingDirectory = dir;
        }

        return resp;
    }

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"WORKDIR {dir}");
        context.WorkingDirectory = dir;

        return Task.CompletedTask;
    }
}
