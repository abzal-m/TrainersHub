namespace TrainersHub.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtMiddleware>();
        }

        public static IApplicationBuilder UseRoleMiddleware(this IApplicationBuilder app, string role)
        {
            return app.UseMiddleware<RoleMiddleware>(role);
        }
    }
}