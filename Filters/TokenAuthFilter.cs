using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using System.Text;
using System;

namespace DotnetBaseApi.Filters
{
    public class TokenAuth : ActionFilterAttribute, IAsyncAuthorizationFilter

    {
        static string key = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";
        public Task OnAuthorizationAsync(AuthorizationFilterContext actionContext)
        {
            var req = actionContext.HttpContext.Request;
            if (req.Headers.ContainsKey("Authorization"))
            {
                var auth = req.Headers["Authorization"][0].Replace("Bearer ","");
                var isValid = ValidateToken(auth);
                Console.WriteLine("Authorization: "+isValid.ToString());
            }
            //User is Authorized, complete execution
            return Task.FromResult<object>(null);

        }
        private static bool ValidateToken(string authToken)
        {
            try {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
            } catch (Exception ex) {
                return false;
            }
            return true;
        }
        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false, // Because there is no audiance in the generated token
                ValidateIssuer = false,   // Because there is no issuer in the generated token
                ValidIssuer = "Sample",
                ValidAudience = "Sample",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) // The same key as the one that generate the token
            };
        }
    }
}