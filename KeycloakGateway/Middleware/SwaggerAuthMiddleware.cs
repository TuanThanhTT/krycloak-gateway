using System.IdentityModel.Tokens.Jwt;

namespace KeycloakGateway.Middleware
{
    public class SwaggerAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                var token = context.Request.Cookies["swagger_token"];

                if (string.IsNullOrEmpty(token))
                {
                    context.Response.Redirect("/swagger-login.html");
                    return;
                }
            }

            await _next(context);
        }
    }
}
