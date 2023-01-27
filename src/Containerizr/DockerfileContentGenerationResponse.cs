namespace Containerizr;

public class DockerfileContentGenerationResponse
{
    private DockerfileContentGenerationResponse() {}

    public static DockerfileContentGenerationResponse Success(string content)
        => new DockerfileContentGenerationResponse { Content = content };
    public static DockerfileContentGenerationResponse Create(Exception exception)
        => new DockerfileContentGenerationResponse { Exception = exception };

    public Exception? Exception { get; init; }
    public string? Content { get; init; }
    public bool IsSuccess => Exception == null;
}
