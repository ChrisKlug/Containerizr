using Containerizr.AspNet;
using Containerizr.Linux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containerizr.Samples.Samples;

internal class DotNetCoreExtensionSample : ISample
{
    public async Task Execute(bool interactive)
    {
        Console.Clear();

        Console.WriteLine("Generating image");

        DockerfileContentGenerationResponse dockerfile;

        using (var image = DebianContainerImage.Create(DotNetCoreImageVersions.SDK_7_0, interactive: interactive))
        {
            await image.AddAspNetAppToImage("./Resources/DemoApi");

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
                Console.WriteLine("Generating image \"containerizr:sample3\"...");

                await image.CreateImage("containerizr", "sample3");

                Console.WriteLine("Done!\r\n\r\nRun container by running: docker run -p 8080:5000 containerizr:sample3\r\n");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    public string Name => "DotNet Core Extension Method";
}
