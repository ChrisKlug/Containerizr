using Containerizr.Directives;

namespace Containerizr;

public static class ContainerImageExtensions
{
    public static async Task<bool> SetWorkingDirectory(this ContainerImage image, string dir)
    {
        var resp = await image.AddDirective(new SetWorkingDirDirective(dir));
        if (resp != DockerDirectiveResponse.NonInteractive && resp.HasError)
        {
            return false;
        }
        image.State.SetItem("BuiltIn.WorkingDir", dir);
        return true;
    }
    public static Task SetEntryPoint(this ContainerImage image, string entrypoint)
        => image.AddDirective(new SetEntryPointDirective(entrypoint));
    public static Task SetCommand(this ContainerImage image, string command)
        => image.AddDirective(new SetCommandDirective(command));
    public static Task<bool> AddFile(this ContainerImage image, string path, string targetPath, string? addName = null)
        => image.AddFiles(new[] { path }, targetPath, addName);
    public static async Task<bool> AddFiles(this ContainerImage image, string[] paths, string targetPath, string? addName = null)
    {
        var resp = await image.AddDirective(new AddFilesDirective(paths, targetPath, addName));
        if (resp != DockerDirectiveResponse.NonInteractive && resp.HasError)
        {
            return false;
        }
        return true;
    }
    public static async Task<bool> AddDirectory(this ContainerImage image, string path, string targetPath, string? addName = null)
    {
        var resp = await image.AddDirective(new AddDirectoryDirective(path, targetPath, addName));
        if (resp != DockerDirectiveResponse.NonInteractive && resp.HasError)
        {
            return false;
        }
        return true;
    }
    public static async Task<bool> CopyFileFromImage(this ContainerImage image, ContainerImage from, string sourcePath, string targetPath, string? fromName = null)
    {
        var resp = await image.AddDirective(new CopyFromImageDirective(from, sourcePath, targetPath, fromName));
        if (resp != DockerDirectiveResponse.NonInteractive && resp.HasError)
        {
            return false;
        }
        return true;
    }
}
