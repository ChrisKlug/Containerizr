namespace Containerizr;

public class ContainerImageConfig
{
    private static int Count = 0;
#if DEBUG
    private const bool InteractiveDefault = true;
#else
    private const bool InteractiveDefault = false;
#endif

    public ContainerImageConfig(string baseImage, bool? interactive)
    {
        BaseImage = baseImage;
        IsInteractive = interactive ?? InteractiveDefault;
        Count++;
    }

    public string BaseImage { get; }
    public bool IsInteractive { get; }
    public string ContainerName { get; init; } = $"containerizr_{DateTime.Now.ToString("HHmmss")}_{Count}";
}