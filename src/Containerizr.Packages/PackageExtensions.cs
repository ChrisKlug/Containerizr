using Containerizr.Directives;
using Containerizr.Linux;

namespace Containerizr.Packages
{
    public static class PackageExtensions
    {
        public static Task<CommandExecutionResponse> AddPackage(this DebianContainerImage image, string packageToInstall, bool skipAptUpdate = false)
            => image.AddDirective(new RunCommandDirective($"{(skipAptUpdate ? "" : "apt-update && ")}apt-get {packageToInstall}"));
        public static Task<CommandExecutionResponse> AddPackage(this CentOSContainerImage image, string packageToInstall)
            => image.AddDirective(new RunCommandDirective($"yum install {packageToInstall}"));
    }
}