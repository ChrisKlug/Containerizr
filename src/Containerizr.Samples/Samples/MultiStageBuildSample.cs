﻿using Containerizr.Linux;

namespace Containerizr.Samples.Samples;

internal class MultiStageBuildSample : ISample
{
    public async Task Execute(bool interactive)
    {
        Console.Clear();

        Console.WriteLine("Generating image");

        DockerfileContentGenerationResponse dockerfile;

        using (var build = DebianContainerImage.Create(DotNetCoreImageVersions.SDK_7_0, interactive: interactive))
        using (var image = DebianContainerImage.Create(DotNetCoreImageVersions.AspNet_Runtime_7_0, interactive: interactive))
        {
            await build.EnsureDirectoryExists("/DemoApi");

            await build.AddDirectory("./Resources/DemoApi", "/DemoApi", "DemoApi");

            await build.SetWorkingDirectory("/DemoApi");

            await build.RunCommand("dotnet publish -c Release -o /DemoApi/Publish");

            await image.EnsureDirectoryExists("/app");

            await image.CopyFileFromImage(build, "/DemoApi/Publish", "/app", "build");

            await image.SetWorkingDirectory("/app");

            await image.SetEntryPoint("dotnet DemoApi.dll");

            dockerfile = await image.GetDockerFileContent();

            if (!dockerfile.IsSuccess)
            {
                Console.Clear();
                Console.Write("Error: " + dockerfile.Exception!.GetBaseException().Message + "\r\n\r\n");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("DOCKERFILE\r\n");
            Console.Write(dockerfile.Content + "\r\n\r\n");
            Console.Write("Generate image (Y/n): ");
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine("Generating image \"containerizr:sample2\"...");

                await image.CreateImage("containerizr", "sample2");

                Console.WriteLine("Done!\r\n\r\nRun container by running: docker run -p 8080:80 containerizr:sample2\r\n");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }

        }
    }

    public string Name => "MultiStage DotNet Core Image";
}