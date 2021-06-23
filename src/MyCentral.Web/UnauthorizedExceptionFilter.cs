using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace MyCentral.Web
{
    public class UnauthorizedExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedException)
            {
                context.ExceptionHandled = true;
                context.Result = new UnauthorizedResult();
            }

            if (context.Exception is DeviceNotFoundException)
            {
                context.ExceptionHandled = true;
                context.Result = new NotFoundResult();
            }

        }
    }
}
