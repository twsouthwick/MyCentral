using System.ComponentModel.DataAnnotations;

namespace CustomService.Client
{
    public class EventClientOptions
    {
        [Required]
        public string ServiceEndpoint { get; set; }
    }
}
