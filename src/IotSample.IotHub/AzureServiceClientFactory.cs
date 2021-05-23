using Azure.Core;
using Microsoft.Azure.Devices;

namespace MyCentral.Client.Azure
{
    public class AzureServiceClientFactory : IServiceClientFactory
    {
        private readonly TokenCredential _credential;

        public AzureServiceClientFactory(TokenCredential credential)
        {
            _credential = credential;
        }

        public IServiceClient CreateClient(string name)
            => new AzureServiceClient(
                name,
                ServiceClient.Create(name, _credential),
                RegistryManager.Create(name, _credential));
    }
}
