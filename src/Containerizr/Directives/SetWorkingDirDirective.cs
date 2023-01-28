namespace Containerizr.Directives;

public class SetWorkingDirDirective : DockerDirective
{
    private readonly string dir;

    public SetWorkingDirDirective(string dir)
    {
        this.dir = dir;
    }

    public override async Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
    {
        var resp = await context.Image.ExecuteCommand($"cd {dir}");
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
