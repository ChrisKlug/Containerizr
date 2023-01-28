namespace Containerizr.Linux;

public class CentOSContainerImage : LinuxContainerImage
{
    private const string DefaultWorkingDir = "/";

    private static readonly Dictionary<CentOSVersions, string> CentOSVersion = new Dictionary<CentOSVersions, string>
    {
        { CentOSVersions.v7, "7" }
    };

    protected CentOSContainerImage(string baseImage, string workingDir, bool? isInteractive = null) 
        : base(baseImage, workingDir, isInteractive) {}

    public static CentOSContainerImage FromImage(string imageName, string workingDir = DefaultWorkingDir, bool? interactive = null, string? tag = null)
        => new CentOSContainerImage($"{imageName}{(tag != null ? $":{tag}" : "")}", workingDir, interactive);
    public static CentOSContainerImage Create(CentOSVersions version, string workingDir = DefaultWorkingDir, bool? interactive = null)
        => new CentOSContainerImage($"centos:{CentOSVersion[version]}", workingDir, interactive);
}
