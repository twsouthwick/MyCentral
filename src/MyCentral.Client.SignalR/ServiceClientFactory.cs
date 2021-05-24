namespace MyCentral.Client.SignalR
{
    public class ServiceClientFactory : IServiceClientFactory
    {
        private readonly SignalrEventClientFactory _eventClientFactory;

        public ServiceClientFactory(SignalrEventClientFactory eventClientFactory)
        {
            _eventClientFactory = eventClientFactory;
        }

        public IServiceClient CreateClient(string hostname, string eventConnectionString)
            => new SignalrServiceClient(hostname, _eventClientFactory.Create(hostname, eventConnectionString));
    }
}
