using Azure.Core;
using System;
using System.Threading.Tasks;

namespace MyCentral.Client.Azure
{
    public class AzureEventClient : IEventClient
    {
        private readonly string _hostname;
        private readonly TokenCredential _credential;

        public AzureEventClient(string hostname, TokenCredential credential)
        {
            _hostname = hostname;
            _credential = credential;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public IDisposable Subscribe(IObserver<Item> observer)
        {
            return EmptyDisposable.Instance;
        }
    }
}
