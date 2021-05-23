using System.ComponentModel.DataAnnotations;

namespace MyCentral.Client.SignalR
{
    public class EventClientOptions
    {
        [Required]
        public string ServiceEndpoint { get; set; }
    }
}
