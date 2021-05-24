namespace MyCentral.Client
{
    public interface IServiceClientFactory
    {
        IServiceClient CreateClient(string name, string eventConnectionString);
    }
}