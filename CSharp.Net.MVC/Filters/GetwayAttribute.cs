using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc.Filters
{
    class GetwayAttribute : ActionFilterAttribute
    {
        ILogger _logger;
        Stopwatch stopwatch;
        private static readonly string key = "enterTime";

        public GetwayAttribute(ILogger<GetwayAttribute> logger)
        {
            _logger = logger;
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
