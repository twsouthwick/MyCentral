using DeviceService.Shared;
using IotSample.IotHub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class Devices : ControllerBase
    {
        private readonly ServiceClientFactory _factory;
        private readonly ILogger<Devices> _logger;

        public Devices(ServiceClientFactory factory, ILogger<Devices> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        [HttpGet("{hostname}")]
        public async IAsyncEnumerable<DeviceInfo> Get(string hostname)
        {
            await using var client = _factory.CreateClient(hostname);

            await foreach (var device in client.GetDevicesAsync(HttpContext.RequestAborted))
            {
                yield return new DeviceInfo
                {
                    DeviceId = device.DeviceId,
                };
            }
        }
    }
}
