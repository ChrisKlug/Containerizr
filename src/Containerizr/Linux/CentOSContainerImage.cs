namespace Containerizr.Linux;

public class CentOSContainerImage : LinuxContainerImage
{
    private const string DefaultWorkingDir = "/";

    protected CentOSContainerImage(string baseImage, string workingDir, bool? isInteractive = null) 
        : base(baseImage, workingDir, isInteractive) {}

    public static CentOSContainerImage FromImage(string imageName, string workingDir = DefaultWorkingDir, string? tag = null, bool? interactive = null)
        => new CentOSContainerImage($"{imageName}{(tag != null ? $":{tag}" : "")}", workingDir, interactive);
}
