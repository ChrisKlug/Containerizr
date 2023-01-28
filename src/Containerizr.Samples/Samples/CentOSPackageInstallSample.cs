using Containerizr.Linux;
using Containerizr.Packages;

namespace Containerizr.Samples.Samples;

internal class CentOSPackageInstallSample : ISample
{
    public async Task Execute(bool interactive)
    {
        Console.Clear();

        Console.WriteLine("Generating image");

        var contextDir = Path.Combine(Path.GetTempPath(), "sample5");
        if (Directory.Exists(contextDir))
        {
            Directory.Delete(contextDir, true);
        }

        using (var image = LinuxContainerImage.Create(CentOSVersions.v7, interactive: interactive))
        {
            await image.AddPackage("iproute");

            await image.SetEntryPoint("ip address show");

            var contextGenerationResult = await image.CreateDockerContext(contextDir);

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
            Console.WriteLine(File.ReadAllText(Path.Combine(contextDir, "dockerfile")) + "\r\n");
            Console.WriteLine($"Temporary container name is: {image.InteractiveContainer.Name}");
            Console.WriteLine($"Context is temporarily available at: {contextDir}\r\n");
            Console.Write("Generate image (Y/n): ");
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine("Generating image \"containerizr:sample5\"...");

                await image.CreateImage("containerizr", "sample5");

                Console.WriteLine("Done!\r\n\r\nRun container by running: docker run containerizr:sample5\r\n");
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

    public string Name => "CentOS Package Installation";
}
