namespace Containerizr.Linux;

public class DebianContainerImage : LinuxContainerImage
{
    private const string DefaultWorkingDir = "/";

    protected DebianContainerImage(string baseImage, string workingDir, bool? isInteractive = null) 
        : base(baseImage, workingDir, isInteractive) {}

    public static DebianContainerImage FromImage(string imageName, string workingDir = DefaultWorkingDir, bool? interactive = null, string? tag = null)
        => new DebianContainerImage($"{imageName}{(tag != null ? $":{tag}" : "")}", workingDir, interactive);
}
