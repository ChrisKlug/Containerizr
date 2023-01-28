namespace Containerizr;

public class ExecutionContext
{
    protected ExecutionContext(ContainerImage image, string workingDirectory)
    {
        Image = image;
        WorkingDirectory = workingDirectory;
    }

    public static ExecutionContext Create(ContainerImage image, string workingDirectory)
        => new ExecutionContext(image, workingDirectory);

    public ContainerImage Image { get; }
    public string WorkingDirectory { get; set; }
}