namespace Containerizr.Samples
{
    internal interface ISample
    {
        Task Execute(bool interactive);

        string Name { get; }
    }
}
