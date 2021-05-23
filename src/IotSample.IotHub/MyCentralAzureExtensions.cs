using Microsoft.Extensions.DependencyInjection;

namespace MyCentral.Client.Azure
{
    public static class MyCentralAzureExtensions
    {
        public static void AddAzureMyCentral(this IServiceCollection services)
        {
            services.AddSingleton<IServiceClientFactory, AzureServiceClientFactory>();
        }
    }
}
