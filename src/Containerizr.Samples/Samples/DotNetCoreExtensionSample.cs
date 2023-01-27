using Containerizr.AspNet;
using Containerizr.Linux;

namespace Containerizr.Samples.Samples;

internal class DotNetCoreExtensionSample : ISample
{
    public async Task Execute(bool interactive)
    {
        Console.Clear();

        Console.WriteLine("Generating image");

        var contextDir = Path.Combine(Path.GetTempPath(), "sample3");
        if (Directory.Exists(contextDir))
        {
            Directory.Delete(contextDir, true);
        }

        using (var image = DebianContainerImage.Create(DotNetCoreImageVersions.SDK_7_0, interactive: interactive))
        {
            await image.AddAspNetAppToImage("./Resources/DemoApi");

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
                Console.WriteLine("Generating image \"containerizr:sample3\"...");

                await image.CreateImage("containerizr", "sample3");

                Console.WriteLine("Done!\r\n\r\nRun container by running: docker run -p 8080:5000 containerizr:sample3\r\n");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }

            Directory.Delete(contextDir, true);
        }
    }

    public string Name => "DotNet Core Extension Method";
}
