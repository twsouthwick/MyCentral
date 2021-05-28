using System;
using System.Collections.Generic;

namespace MyCentral.Client
{
    public record Event(
        string DeviceId,
        DateTimeOffset EnqueuedTime,
        string Body,
        IEnumerable<KeyValuePair<string, object>> Properties,
        IEnumerable<KeyValuePair<string, object>> SystemProperties);
}
