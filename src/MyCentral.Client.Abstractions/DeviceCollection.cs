using System.Collections.Generic;

namespace MyCentral.Client
{
    public record DeviceCollection
    {
        public ICollection<DeviceInstance> Devices { get; init; } = new List<DeviceInstance>();
    }
}
