﻿using MyCentral.Device.Emulation;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client
{
    public class ClientEmulatedDeviceManager : IEmulatedDeviceManager
    {
        private readonly IServiceClient _client;
        private readonly HttpClient _http;

        public ClientEmulatedDeviceManager(IServiceClient client, HttpClient http)
        {
            _client = client;
            _http = http;
        }

        public DeviceCollection Collection => throw new NotImplementedException();

        public Task<IEmulatedDevice?> GetDeviceAsync(string id, CancellationToken token)
            => Task.FromResult<IEmulatedDevice?>(new EmulatedDevice(_http, id));

        private class EmulatedDevice : IEmulatedDevice
        {
            private readonly HttpClient _client;
            private readonly string _name;

            public EmulatedDevice(HttpClient client, string name)
            {
                _client = client;
                _name = name;
            }

            public ValueTask DisposeAsync()
            {
                return new ValueTask();
            }

            public Task SendAsync(string componentName, Stream content)
            {
                throw new NotImplementedException();
            }

            public Task SendAsync(string componentName, string content)
            {
                return _client.PostAsync($"api/devices/{_name}/{componentName}", new StringContent(content));
            }
        }
    }
}