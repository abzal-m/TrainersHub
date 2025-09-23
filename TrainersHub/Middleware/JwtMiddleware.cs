using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TrainersHub.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secret;

        public JwtMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _secret = config["Jwt:Key"] ?? throw new Exception("Jwt:Key not configured");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
                AttachUserToContext(context, token);

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var claims = new ClaimsIdentity(jwtToken.Claims, "jwt");
                var principal = new ClaimsPrincipal(claims);

                context.User = principal;
            }
            catch
            {
                // токен невалидный, пользователь останется не аутентифицирован
            }
        }
    }
}