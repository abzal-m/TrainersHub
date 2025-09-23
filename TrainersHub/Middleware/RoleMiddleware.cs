using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TrainersHub.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _requiredRole;

        public RoleMiddleware(RequestDelegate next, string requiredRole)
        {
            _next = next;
            _requiredRole = requiredRole;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.User.Identity?.IsAuthenticated ?? false)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == null || role != _requiredRole)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }

            await _next(context);
        }
    }
}