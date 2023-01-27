using System.Diagnostics;

namespace Containerizr.Directives;

public class DockerDirectiveResponse
{
    public static DockerDirectiveResponse NonInteractive = new DockerDirectiveResponse();
    public static DockerDirectiveResponse Empty = new DockerDirectiveResponse();

    public static async Task<DockerDirectiveResponse> Create(Process process)
    {
        var stdOut = await process.StandardOutput.ReadToEndAsync();
        var errOut = await process.StandardError.ReadToEndAsync();

        return new DockerDirectiveResponse { StdOut = stdOut, ErrOut = errOut };
    }
    public static DockerDirectiveResponse Create(string? stdOut, string? errOut)
        => new DockerDirectiveResponse { StdOut = stdOut, ErrOut = errOut };

    public string? StdOut { get; init; }
    public string? ErrOut { get; init; }
    public bool HasError => !string.IsNullOrEmpty(ErrOut);
}
