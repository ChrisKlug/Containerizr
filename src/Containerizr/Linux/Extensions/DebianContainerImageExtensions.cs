using Containerizr.Directives;
using Containerizr.Linux;
using Containerizr.Linux.Directives;

namespace Containerizr;

public static class DebianContainerImageExtensions
{
    public static async Task<bool> RunCommand(this DebianContainerImage img, string command)
    {
        var ret = await img.AddDirective(new RunCommandDirective(ctx => $"bash -c \"cd {ctx.WorkingDirectory} && {command.Replace("\"", "\\\"")}\""));
        return ret == DockerDirectiveResponse.NonInteractive || !ret.HasError;
    }
    public static async Task<bool> EnsureDirectoryExists(this DebianContainerImage img, string path)
    {
        var ret = await img.AddDirective(new EnsureDirectoryDirective(path));
        return ret == DockerDirectiveResponse.NonInteractive || !ret.HasError;
    }
    public static async Task<bool> SetEnvironmentVariable(this DebianContainerImage img, string name, string value)
    {
        var ret = await img.AddDirective(new SetEnvironmentVariableDirective(name, value));
        return ret == DockerDirectiveResponse.NonInteractive || !ret.HasError;
    }
}
