namespace UserManagementAPI.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    private const string ValidToken =
        "my-secret-token";

    public AuthenticationMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(
                "Authorization",
                out var token))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Missing authorization token."
            });

            return;
        }

        var receivedToken =
            token.ToString().Replace("Bearer ", "");

        if (receivedToken != ValidToken)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid token."
            });

            return;
        }

        await _next(context);
    }
}