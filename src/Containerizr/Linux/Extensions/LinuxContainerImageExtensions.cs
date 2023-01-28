using Containerizr.Directives;
using Containerizr.Linux;
using Containerizr.Linux.Directives;

namespace Containerizr;

public static class LinuxContainerImageExtensions
{
    public static async Task<bool> EnsureDirectoryExists(this LinuxContainerImage img, string path)
    {
        var ret = await img.AddDirective(new EnsureDirectoryDirective(path));
        return ret == CommandExecutionResponse.NonInteractive || !ret.HasError;
    }
    public static async Task<bool> SetEnvironmentVariable(this LinuxContainerImage img, string name, string value)
    {
        var ret = await img.AddDirective(new SetEnvironmentVariableDirective(name, value));
        return ret == CommandExecutionResponse.NonInteractive || !ret.HasError;
    }
}
