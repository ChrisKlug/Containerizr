using Containerizr.Linux;

namespace Containerizr.AspNet
{
    public static class DebianContainerImageExtensions
    {
        public static async Task AddAspNetAppToImage(this LinuxContainerImage image, string pathToAppSource)
        {
            await image.EnsureDirectoryExists("/app");

            await image.AddDirectory(pathToAppSource, "/app");

            await image.SetWorkingDirectory("/app");

            await image.SetEnvironmentVariable("DOTNET_URLS", "http://+:5000");

            await image.SetEntryPoint("dotnet run");
        }
    }
}