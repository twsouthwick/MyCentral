using Microsoft.AspNetCore.Mvc;
using MyCentral.Client;
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
        public Task<DeviceCollection> GetDevices() => _client.GetDevicesAsync(HttpContext.RequestAborted);
    }
}
