using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyCentral.Client.SignalR
{
    public class SignalrEventClientFactory
    {
        private readonly IOptions<EventClientOptions> _options;
        private readonly ILoggerProvider _loggerProvider;

        public SignalrEventClientFactory(IOptions<EventClientOptions> options, ILoggerProvider loggerProvider)
        {
            _options = options;
            _loggerProvider = loggerProvider;
        }

        public SignalrEventClient Create(string hostname) => new(_options, _loggerProvider, hostname);
    }
}
