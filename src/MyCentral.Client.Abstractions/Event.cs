using System;

namespace MyCentral.Client
{
    public record Event(
        string DeviceId,
        string Subject,
        DateTimeOffset EnqueuedTime,
        string Body);
}
