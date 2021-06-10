using Microsoft.Extensions.Options;
using MyCentral.Client;

namespace MyCentral.Device.Emulation
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
