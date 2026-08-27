using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DotnetBaseApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ETagFilter : Attribute, IAsyncResultFilter
    {
        private readonly int[] _statusCodes;

        public ETagFilter(params int[] statusCodes)
        {
            _statusCodes = statusCodes;
            if (statusCodes.Length == 0) _statusCodes = [200];
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var originalBodyStream = context.HttpContext.Response.Body;
        
            using var memoryStream = new MemoryStream();
            context.HttpContext.Response.Body = memoryStream;
            
            await next();  // Execute the result - this is when status code is finalized
            
            // Now check the actual status code
            if (_statusCodes.Contains(context.HttpContext.Response.StatusCode))
            {
                var responseBytes = memoryStream.ToArray();
                if (responseBytes.Length > 0)
                {
                    var etag = ETagGenerator.GenerateETag(responseBytes);
                    
                    if (context.HttpContext.Request.Method == "GET" && 
                        context.HttpContext.Request.Headers.TryGetValue("If-None-Match", out var value) && 
                        value.ToString() == etag)
                    {
                        context.HttpContext.Response.StatusCode = 304;
                        context.HttpContext.Response.ContentLength = 0;
                    }
                    else
                    {
                        context.HttpContext.Response.Headers.ETag = etag;
                        await originalBodyStream.WriteAsync(responseBytes);
                    }
                }
            }
            else
            {
                // If status code doesn't match, still write the bytes
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            
            context.HttpContext.Response.Body = originalBodyStream;
        }
    }
    public static class ETagGenerator
    {
        public static string GenerateETag(byte[] data, bool weak = false)
        {
            var hash = MD5.HashData(data);
            var etag = Convert.ToHexString(hash);
            return weak ? $"W/\"{etag}\"" : $"\"{etag}\"";
        }
    }
}