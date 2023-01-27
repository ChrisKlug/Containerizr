namespace Containerizr;

public class ExecutionContext
{
    protected ExecutionContext(ContainerImageConfig config, string workingDirectory)
    {
        ContainerConfig = config;
        WorkingDirectory = workingDirectory;
    }

    public static ExecutionContext Create(ContainerImageConfig config, string workingDirectory)
        => new ExecutionContext(config, workingDirectory);

    public ContainerImageConfig ContainerConfig { get; }
    public string WorkingDirectory { get; set; }
}