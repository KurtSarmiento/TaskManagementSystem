using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagementSystem.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class APIKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;

            if (!context.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "API Key was not provided."
                };
                return;
            }

            //if (configuration == null || string.IsNullOrEmpty(configuration["Security:ApiKey"])) **ONLY FOR SINGULAR API KEY**
            //{
            //    context.Result = new ContentResult()
            //    {
            //        StatusCode = 500,
            //        Content = "Server configuration error: API Key not found."
            //    };
            //    return;
            //}

            //var apiKey = configuration["Security:ApiKey"]; **ONLY FOR SINGULAR API KEY**

            //if (!apiKey.Equals(extractedApiKey))
            //{
            //    context.Result = new ContentResult()
            //    {
            //        StatusCode = 401,
            //        Content = "Invalid API Key."
            //    };
            //    return;
            //}

            if (configuration == null)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 500,
                    Content = "Server configuration error: API Key not found."
                };
                return;
            }

            var validKeys = configuration.GetSection("Security:ApiKeys").Get<List<string>>();

            if(validKeys == null || validKeys.Count() != 0)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 500,
                    Content = "Server configuration error: API Keys not found."
                };
                return;
            }

            if (!validKeys.Contains(extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Invalid API Key."
                };
                return;
            }
        }
    }
}