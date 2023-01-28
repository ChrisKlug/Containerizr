using Containerizr.Linux;

namespace Containerizr.Samples.Samples;

internal class DotNetCoreSample : ISample
{
    public async Task Execute(bool interactive)
    {
        Console.Clear();

        Console.WriteLine("Generating image");

        var contextDir = Path.Combine(Path.GetTempPath(), "sample1");
        if (Directory.Exists(contextDir))
        {
            Directory.Delete(contextDir, true);
        }

        using (var image = DebianContainerImage.Create(DotNetCoreImageVersions.SDK_7_0, interactive: interactive))
        {
            await image.EnsureDirectoryExists("/DemoApi");

            await image.AddDirectory("./Resources/DemoApi", "/DemoApi", "DemoApi");

            await image.SetWorkingDirectory("/DemoApi");

            await image.SetEnvironmentVariable("DOTNET_URLS", "http://+:5000");

            await image.SetEntryPoint("dotnet run");

            var contextGenerationResult = await image.CreateDockerContext(contextDir, true);

            if (!contextGenerationResult.IsSuccess)
            {
                Console.Clear();
                Console.Write("Error: " + contextGenerationResult.Exception!.GetBaseException().Message + "\r\n\r\n");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("DOCKERFILE\r\n");
            Console.Write(File.ReadAllText(Path.Combine(contextDir, "dockerfile")) + "\r\n\r\n");
            Console.Write($"Context is temporarily available at: {contextDir}\r\n\r\n");
            Console.Write("Generate image (Y/n): ");
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine("Generating image \"containerizr:sample1\"...");

                await image.CreateImage("containerizr", "sample1");

                Console.WriteLine("Done!\r\n\r\nRun container by running: docker run -p 8080:5000 containerizr:sample1\r\n");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }

            Console.Clear();
            Console.Write("Deleting temporary container...");
        }

        Console.WriteLine("Done!");
        Console.WriteLine("Deleting temporary context...");
        Directory.Delete(contextDir, true);
    }

    public string Name => "DotNet Core SDK Image";
}
