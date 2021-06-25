using System;
using System.Net;
using System.Net.Http;

namespace MyCentral.Client
{
    internal static class HttpExtensions
    {
        public static void ThrowIfFailed(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => throw new UnauthorizedAccessException(),
                var code => throw new InvalidOperationException(code.ToString()),
            };
        }
    }
}
