namespace Containerizr.Linux;

public abstract class LinuxContainerImage : ContainerImage
{
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
        { DotNetCoreImageVersions.SDK_6_0, "mcr.microsoft.com/dotnet/sdk" },
        { DotNetCoreImageVersions.SDK_7_0, "mcr.microsoft.com/dotnet/sdk" },
        { DotNetCoreImageVersions.AspNet_Runtime_6_0, "mcr.microsoft.com/dotnet/aspnet" },
        { DotNetCoreImageVersions.AspNet_Runtime_7_0, "mcr.microsoft.com/dotnet/aspnet" }
    };
    private static readonly Dictionary<DotNetCoreImageVersions, string> DotNetCoreTags = new Dictionary<DotNetCoreImageVersions, string>
    {
        { DotNetCoreImageVersions.SDK_6_0, "6.0" },
        { DotNetCoreImageVersions.SDK_7_0, "7.0" },
        { DotNetCoreImageVersions.AspNet_Runtime_6_0, "6.0" },
        { DotNetCoreImageVersions.AspNet_Runtime_7_0, "7.0" }
    };
    private static readonly Dictionary<CentOSVersions, string> CentOSVersion = new Dictionary<CentOSVersions, string>
    {
        { CentOSVersions.v7, "7" }
    };

    protected LinuxContainerImage(string baseImage, string initialWorkingDirectory, bool? isInteractive = null) 
        : base(baseImage, initialWorkingDirectory, isInteractive)
    {
    }

    protected internal override string FormatCommand(string command, string currentDirectory) 
        => $"bash -c \"cd {currentDirectory} && {command.Replace("\"", "\\\"")}\"";

    public static CentOSContainerImage Create(CentOSVersions version, bool? interactive = null)
        => CentOSContainerImage.FromImage("centos", tag: CentOSVersion[version], interactive: interactive);
    public static DebianContainerImage Create(UbuntuVersion version, bool? interactive = null)
        => DebianContainerImage.FromImage($"ubuntu", tag: UbuntuVersionTag[version], interactive: interactive);
    public static DebianContainerImage Create(DotNetCoreImageVersions version, bool? interactive = null)
        => DebianContainerImage.FromImage(DotNetCoreImageNames[version], tag: DotNetCoreTags[version], interactive: interactive);
}
