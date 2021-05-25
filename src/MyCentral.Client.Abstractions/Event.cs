using System;
using System.Collections.Generic;

namespace MyCentral.Client
{
    public record Event(DateTimeOffset EnqueuedTime, IDictionary<string, object> Properties, IReadOnlyDictionary<string, object> SystemProperties);
}
