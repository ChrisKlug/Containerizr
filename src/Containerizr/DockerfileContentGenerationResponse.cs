namespace Containerizr;

public class DockerfileContentGenerationResponse
{
    private DockerfileContentGenerationResponse(string contextDirectory) 
    {
        ContextDirectory = contextDirectory;
    }
    private DockerfileContentGenerationResponse(string content, string contextDirectory)
        :this(contextDirectory)
    {
        Content = content;
    }
    private DockerfileContentGenerationResponse(Exception exception, string contextDirectory)
        : this(contextDirectory)
    {
        Exception = exception;
    }

    public static DockerfileContentGenerationResponse Success(string content, string contextDirectory)
        => new DockerfileContentGenerationResponse(content, contextDirectory);
    public static DockerfileContentGenerationResponse Create(Exception exception, string contextDirectory)
        => new DockerfileContentGenerationResponse(exception, contextDirectory);

    public Exception? Exception { get; init; }
    public string? Content { get; init; }
    public string ContextDirectory { get; init; }
    public bool IsSuccess => Exception == null;
}
