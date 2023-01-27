namespace Containerizr;

public static class ContainerImageStateExtensions
{
    public static string GetWorkingDirectory(this ContainerImageState imageState)
        => imageState.GetItem<string>("BuiltIn.WorkingDir")!;
    public static string GetContextDirectory(this ContainerImageState imageState)
        => imageState.GetItem<string>("BuiltIn.ContextDir")!;
}
