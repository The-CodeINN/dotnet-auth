using System.IdentityModel.Tokens.Jwt;
using dotnet_auth.Services;

namespace dotnet_auth.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken != null)
                {
                    var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                    var user = await userService.GetUserAsync(userId);

                    if (user != null)
                    {
                        context.Items["User"] = user;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JWT validation failed: {ex.Message}");
            }
        }

        await _next(context);
    }
}