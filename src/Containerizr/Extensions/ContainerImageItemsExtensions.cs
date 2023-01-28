namespace Containerizr;

public static class ContainerImageItemsExtensions
{
    public static string GetWorkingDirectory(this ContainerImageItems imageState)
        => imageState.GetItem<string>("BuiltIn.WorkingDir")!;
    public static string GetContextDirectory(this ContainerImageItems imageState)
        => imageState.GetItem<string>("BuiltIn.ContextDir")!;
}
