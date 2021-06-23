using Microsoft.AspNetCore.Mvc;
using MyCentral.Client;
using MyCentral.Device.Emulation;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
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

        [HttpPost("{deviceId}/invoke/{method}")]
        public async Task<ActionResult> InvokeMethod(string deviceId, string method)
        {
            var content = await ReadBodyContents();

            var result = await _client.InvokeMethodAsync(deviceId, method, content);

            if (result == "null")
            {
                return NotFound();
            }

            return Content(result, "application/json");
        }

        private async Task<string> ReadBodyContents()
        {
            using var reader = new StreamReader(Request.Body);
            return await reader.ReadToEndAsync();
        }

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
