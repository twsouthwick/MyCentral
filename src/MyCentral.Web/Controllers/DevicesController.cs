using Microsoft.AspNetCore.Mvc;
using MyCentral.Client;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MyCentral.Web
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IServiceClient _client;

        public DevicesController(IServiceClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IAsyncEnumerable<string> GetDevices() => _client.GetDevicesAsync(HttpContext.RequestAborted);

        [HttpPost("{deviceId}/invoke/{method}")]
        public async Task<ActionResult> InvokeMethod(string deviceId, string method)
        {
            var content = await ReadBodyContentsAsync();

            var result = await _client.InvokeMethodAsync(deviceId, method, content);

            if (result == "null")
            {
                return BadRequest();
            }

            return Content(result, "application/json");
        }

        private async Task<string> ReadBodyContentsAsync()
        {
            using var reader = new StreamReader(Request.Body);
            return await reader.ReadToEndAsync();
        }

        [HttpPost("{deviceId}/{componentName}/{propertyName}/{propertyValue}")]
        public async Task<ActionResult> UpdatePropertyAsync(string deviceId, string componentName, string propertyName, string propertyValue)
        {
            await _client.UpdatePropertyAsync(deviceId, componentName, propertyName, propertyValue);

            return NoContent();
        }
    }
}
