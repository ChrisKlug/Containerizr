using Containerizr.Directives;
using System.Diagnostics;

namespace Containerizr;

public class InteractiveContainer : IDisposable
{
    private readonly ContainerImage image;
    private readonly string containerName;
    private readonly Func<(string WorkingDirectory, string Command), string> commandFomatter;
    private Process? interactiveContainer;
    private ExecutionContext context;
    private bool isDisposed;

    public InteractiveContainer(ContainerImage image, 
                                string containerName, 
                                string initialWorkingDirectory, 
                                Func<(string WorkingDirectory, string Command), string> commandFomatter,
                                bool enabled)
    {
        this.image = image;
        this.containerName = containerName;
        this.commandFomatter = commandFomatter;
        this.IsEnabled = enabled;
        this.context = ExecutionContext.Create(image, initialWorkingDirectory);
    }

    protected internal Task<CommandExecutionResponse> ExecuteCommand(string command)
    {
        return ExecuteDockerCommand($"exec {containerName} {commandFomatter((Context.WorkingDirectory, command))}");
    }
    protected internal async Task<CommandExecutionResponse> ExecuteDockerCommand(string command)
    {
        if (!IsEnabled)
        {
            return CommandExecutionResponse.NonInteractive;
        }

        var si = new ProcessStartInfo("docker", command)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var process = Process.Start(si)!;

        await process.WaitForExitAsync();

        return await CommandExecutionResponse.Create(process);
    }

    internal async Task Initialize()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (interactiveContainer == null)
        {
            var si = new ProcessStartInfo("docker", $"run -i --rm --name {containerName} {image.BaseImage}")
            {
                CreateNoWindow = true,
            };
            interactiveContainer = Process.Start(si)!;

            si = new ProcessStartInfo("docker", "container inspect -f '{{.State.Running}}' " + containerName)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            var isRunning = false;
            Process process;
            while (!isRunning)
            {
                process = Process.Start(si)!;
                await process.WaitForExitAsync();
                var response = await process.StandardOutput.ReadLineAsync();
                isRunning = response == "'true'";
            }
        }
    }

    public string Name => containerName;
    public bool IsEnabled { get; }
    public ExecutionContext Context => context;

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                if (interactiveContainer != null)
                {
                    interactiveContainer.Close();

                    var si = new ProcessStartInfo("docker", $"stop {containerName}")
                    {
                        CreateNoWindow = true,
                    };
                    Process.Start(si)!.WaitForExit();
                }
            }

            isDisposed = true;
        }
    }

    ~InteractiveContainer()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
