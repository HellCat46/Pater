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
        byte[]? UserData = context.Session.Get("UserData");
        
        Console.WriteLine(path, UserData);
        if (path.StartsWith("/User") && UserData == null)
            context.Response.Redirect("/Home/Login");
        else if (path.StartsWith("/Admin") && UserData == null)
            context.Response.Redirect("/");
        else if (path == "/" && UserData != null)
            context.Response.Redirect("/User/Dashboard");
        
        
        await _next(context);
    }
}