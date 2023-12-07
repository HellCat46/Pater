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
        var SessionID = context.Session.GetString("SessionID");
        
        Console.WriteLine(path, SessionID);
        if (path.StartsWith("/User") && SessionID == null)
            context.Response.Redirect("/Home/Login");
        else if (path.StartsWith("/Admin") && SessionID == null)
            context.Response.Redirect("/");
        else if (path == "/" && SessionID != null)
            context.Response.Redirect("/User/Dashboard");
        
        
        await _next(context);
    }
}