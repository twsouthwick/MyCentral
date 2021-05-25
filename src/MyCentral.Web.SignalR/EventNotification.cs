namespace MyCentral.Web.Hubs
{
    public record EventNotification(EventState State, string ConnectionId)
    {
        public string? Host { get; init; }

        public string? EventConnectionString { get; init; }
    }
}
