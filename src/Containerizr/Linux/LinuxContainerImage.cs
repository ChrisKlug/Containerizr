namespace Containerizr.Linux
{
    public abstract class LinuxContainerImage : ContainerImage
    {
        protected LinuxContainerImage(string baseImage, string workingDir, bool? isInteractive = null) 
            : base(baseImage, workingDir, isInteractive)
        {
        }

        protected override string FormatCommand(string command, string currentDirectory) 
            => $"bash -c \"cd {currentDirectory} && {command.Replace("\"", "\\\"")}\"";
    }
}
