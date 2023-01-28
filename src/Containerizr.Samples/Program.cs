using Containerizr;
using Containerizr.Linux;
using Containerizr.Samples;
using Containerizr.Samples.Samples;
using System.Runtime.CompilerServices;

var samples = new Dictionary<string, ISample>
{
    { "1", new DotNetCoreSample() },
    { "2", new MultiStageBuildSample() },
    { "3", new DotNetCoreExtensionSample() }
};

string input;
do
{
    Console.WriteLine("Choose Sample\r\n");

    foreach (var sample in samples)
    {
        Console.WriteLine($"{sample.Key}. {sample.Value.Name}");
    }

    Console.WriteLine($"q. Exit");

    Console.Write($"\r\nSelect: ");

    input = Console.ReadLine() ?? "";

    if (!string.IsNullOrWhiteSpace(input) && samples.ContainsKey(input))
    {
        await samples[input].Execute(true);
    }

    Console.Clear();

} while (input != "q");
