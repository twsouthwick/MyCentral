using System.ComponentModel.DataAnnotations;

namespace MyCentral.Client
{
    public record DeviceInstance
    {
        [Required]
        public string Name { get; init; } = string.Empty;

        [Required]
        public string Key { get; init; } = string.Empty;

        [Required]
        public string ModelId { get; init; } = string.Empty;

        public string GetConnectionString(string hostname) => $"HostName={hostname};DeviceId={Name};SharedAccessKey={Key}";
    }
}
