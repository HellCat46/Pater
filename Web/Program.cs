using Web;
using Microsoft.EntityFrameworkCore;
using Web.ApplicationDbContext;
using Web.Models.Link;
using Web.Models.View;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("prod")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "SessionID";
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();
app.UseMiddleware<Middleware>();

app.MapControllerRoute(
    name: "web",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/{code}", async (HttpContext context, string code, UserDbContext db) =>
{
    Console.Write(context.Connection.RemoteIpAddress);
    string redirectUrl = "/";
    try
    {
        var linkObj  = db.Link.Find(code);
        if (linkObj == null) throw new Exception();
        redirectUrl = linkObj.url;

        
        Uri url = new Uri("http://ip-api.com/json/"+context.Connection.RemoteIpAddress+"?fields=status,country,city");
        var info = await new HttpClient().GetFromJsonAsync<VisitorGeoLoc>(url);
        var (browser, device, os) = AnalyticsModel.ParseUserAgent(context.Request.Headers.UserAgent);
        
        if (info is {status: "success"})
        {
            db.Analytics.Add(new AnalyticsModel()
            {
                city = info.city,
                country = info.country,
                LinkModelCode = code,
                browser = browser,
                device = device,
                os = os,
                visitedAt = DateTime.Now,
            });
            db.SaveChanges();
        }
        
    }
    catch (Exception ex)
    {
        Console.Write(ex);
    }
    return Results.Redirect(redirectUrl);
});
app.Run();
