using System.Diagnostics;

namespace Containerizr.Directives;

public class CommandExecutionResponse
{
    public static CommandExecutionResponse NonInteractive = new CommandExecutionResponse();
    public static CommandExecutionResponse Empty = new CommandExecutionResponse();

    public static async Task<CommandExecutionResponse> Create(Process process)
    {
        var stdOut = await process.StandardOutput.ReadToEndAsync();
        var errOut = await process.StandardError.ReadToEndAsync();

        return new CommandExecutionResponse { StdOut = stdOut, ErrOut = errOut };
    }
    public static CommandExecutionResponse Create(string? stdOut, string? errOut)
        => new CommandExecutionResponse { StdOut = stdOut, ErrOut = errOut };

    public string? StdOut { get; init; }
    public string? ErrOut { get; init; }
    public bool HasError => !string.IsNullOrEmpty(ErrOut);
}
