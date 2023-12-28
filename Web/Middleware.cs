using Microsoft.IdentityModel.Tokens;

namespace Web;

public class Middleware
{
    private readonly RequestDelegate _next;

    public Middleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        String path = context.Request.Path.ToString();
        bool userData = context.Session.Get("UserData").IsNullOrEmpty();
        
        if (path.StartsWith("/User") && userData)
            context.Response.Redirect("/Home/Login");
        else if (path.StartsWith("/Admin") && userData)
            context.Response.Redirect("/");
        else if (path == "/" && !userData)
            context.Response.Redirect("/User/Dashboard");
        
        
        await _next(context);
    }
}