using Microsoft.AspNetCore.Mvc;
using MyCentral.Client;
using MyCentral.Device.Emulation;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MyCentral.Web
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IServiceClient _client;
        private readonly IEmulatedDeviceManager _emulatedDevices;

        public DevicesController(IServiceClient client, IEmulatedDeviceManager emulatedDevices)
        {
            _client = client;
            _emulatedDevices = emulatedDevices;
        }

        [HttpGet]
        public Task<DeviceCollection> GetDevices() => _client.GetDevicesAsync(HttpContext.RequestAborted);

        [HttpPost("{deviceId}/{componentName}")]
        public async Task<ActionResult> SendValue(string deviceId, [Required] string componentName)
        {
            var device = await _emulatedDevices.GetDeviceAsync(deviceId, HttpContext.RequestAborted);

            if (device is null)
            {
                return NotFound(new { DeviceId = deviceId });
            }

            await device.SendAsync(componentName, Request.Body);

            return NoContent();
        }
    }
}
