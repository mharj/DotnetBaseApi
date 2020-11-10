using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotnetBaseApi.Filters
{
    public class TokenAuth : ActionFilterAttribute, IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext actionContext)
        {
            var req = actionContext.HttpContext.Request;
            if (req.Headers.ContainsKey("Authorization"))
            {
                // TODO check
            }


            //User is Authorized, complete execution
            return Task.FromResult<object>(null);

        }
    }
}