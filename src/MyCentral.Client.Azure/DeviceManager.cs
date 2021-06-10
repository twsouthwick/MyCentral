using Microsoft.Extensions.Options;

namespace MyCentral.Client.Azure
{
    public class DeviceManager
    {
        public DeviceManager(IOptions<DeviceCollection> collection)
        {
            Collection = collection.Value;
        }

        public DeviceCollection Collection { get; }
    }
}
