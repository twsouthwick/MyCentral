using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCentral.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyCentral.Browser
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("DeviceService.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
                //.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DeviceService.ServerAPI"));

            builder.Services.AddSignalRMyCentral(options =>
            {
                options.ServiceEndpoint = builder.HostEnvironment.BaseAddress;
            });

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("api://3150ef55-cad8-4a17-b4ed-b0118fbe98ed/access_as_user");
            });

            await builder.Build().RunAsync();
        }
    }
}
