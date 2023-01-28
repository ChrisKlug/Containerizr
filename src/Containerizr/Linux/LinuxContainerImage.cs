namespace Containerizr.Linux
{
    public abstract class LinuxContainerImage : ContainerImage
    {
        protected LinuxContainerImage(string baseImage, string initialWorkingDirectory, bool? isInteractive = null) 
            : base(baseImage, initialWorkingDirectory, isInteractive)
        {
        }

        protected internal override string FormatCommand(string command, string currentDirectory) 
            => $"bash -c \"cd {currentDirectory} && {command.Replace("\"", "\\\"")}\"";
    }
}
