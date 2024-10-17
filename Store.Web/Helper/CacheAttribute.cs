using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Store.Services.Services.CashService;
using System.Text;

namespace Store.Web.Helper
{
    public class CacheAttribute : Attribute,IAsyncActionFilter
    {
        private readonly int _timeToLiveInSeconds;

        public CacheAttribute(int timeToLiveInSeconds)
        {
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var _cashService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            var cashKey = GenerateCashKeyFromRequest(context.HttpContext.Request);
            var cashedResponse = await _cashService.GetCacheResponseAsync(cashKey);
            if(!string.IsNullOrEmpty(cashedResponse))
            {
                var contentResult = new ContentResult
                {
                    Content = cashedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };

                context.Result = contentResult;
                return;
            }
            
            var executedContext = await next();
            if (executedContext.Result is OkObjectResult response)
            {
                await _cashService.SetCacheResponseAsync(cashKey, response.Value,TimeSpan.FromSeconds(_timeToLiveInSeconds));
            }
        }

        private string GenerateCashKeyFromRequest(HttpRequest request)
        {
            StringBuilder cashKey = new StringBuilder();
            cashKey.Append($"{request.Path}");

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                cashKey.Append($"|{key}-{value}");
            }
            return cashKey.ToString();
        }
    }
}
