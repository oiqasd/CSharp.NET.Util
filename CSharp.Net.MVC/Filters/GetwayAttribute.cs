using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
        public override void OnActionExecuting(ActionExecutingContext context)
        {

        }
    }
}
