namespace Containerizr.Linux;

public class DebianContainerImage : ContainerImage
{
    private const string DefaultWorkingDir = "/";

    private static readonly Dictionary<UbuntuVersion, string> UbuntuVersionTag = new Dictionary<UbuntuVersion, string>
    {
        { UbuntuVersion.Xenial_16_04, "xenial" },
        { UbuntuVersion.Bionic_18_04, "bionic" },
        { UbuntuVersion.Focal_20_04, "focal" },
        { UbuntuVersion.Jammy_22_04, "jammy" },
        { UbuntuVersion.Kinetic_22_10, "kinetic" },
        { UbuntuVersion.Lunar_23_04, "lunar" },
    };

    private static readonly Dictionary<DotNetCoreImageVersions, string> DotNetCoreImageNames = new Dictionary<DotNetCoreImageVersions, string>
    {
        { DotNetCoreImageVersions.SDK_6_0, "mcr.microsoft.com/dotnet/sdk:6.0" },
        { DotNetCoreImageVersions.SDK_7_0, "mcr.microsoft.com/dotnet/sdk:7.0" },
        { DotNetCoreImageVersions.AspNet_Runtime_6_0, "mcr.microsoft.com/dotnet/aspnet:6.0" },
        { DotNetCoreImageVersions.AspNet_Runtime_7_0, "mcr.microsoft.com/dotnet/aspnet:7.0" }
    };

    protected DebianContainerImage(ContainerImageConfig config, string workingDir) : base(config, workingDir) {}

    public static DebianContainerImage FromImage(string imageName, string workingDir = DefaultWorkingDir, bool? interactive = null, string? tag = null)
        => new DebianContainerImage(new ContainerImageConfig($"{imageName}{(tag != null ? $":{tag}" : "")}", interactive), workingDir);
    public static DebianContainerImage Create(UbuntuVersion version, string workingDir = DefaultWorkingDir, bool? interactive = null)
        => new DebianContainerImage(new ContainerImageConfig($"ubuntu:{UbuntuVersionTag[version]}", interactive), workingDir);
    public static DebianContainerImage Create(DotNetCoreImageVersions version, string workingDir = DefaultWorkingDir, bool? interactive = null)
        => new DebianContainerImage(new ContainerImageConfig(DotNetCoreImageNames[version], interactive), workingDir);
}
