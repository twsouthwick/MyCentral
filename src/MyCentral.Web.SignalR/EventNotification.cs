namespace MyCentral.Web.Hubs
{
    public record EventNotification(EventState State, string ConnectionId)
    {
    }
}
