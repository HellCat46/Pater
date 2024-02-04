using Microsoft.EntityFrameworkCore;
using Web;
using Web.ApplicationDbContext;
using Web.Models.Link;
using Web.Models.View;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration["DB:ConnectionString"]));

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
    "UserPages",
    "{area}/{action:exists}/{id?}",
    new { area = "User", controller = "User" });

app.MapControllerRoute(
    "AdminPages",
    "{area}/{action:exists}/{id?}",
    new { area = "Admin", controller = "Admin"});

app.MapControllerRoute(
    "APIEndpoints",
    "{area:exists}/{controller=Api}/{action}/{id?}");

app.MapControllerRoute(
    "web",
    "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/{code}", (HttpContext context, string code, UserDbContext db) =>
{
    return Task.Run(async () =>
    {
        Console.Write(context.Connection.RemoteIpAddress);
        string? redirectUrl = null;
        try
        {
            var url = new Uri("http://ip-api.com/json/" + context.Connection.RemoteIpAddress +
                              "?fields=status,country,city");
            var geoLocTask = new HttpClient().GetFromJsonAsync<VisitorGeoLoc>(url);


            redirectUrl = await db.Link.Where(link => link.code == code).Select(link => link.url).FirstAsync();
            if (redirectUrl == null) throw new Exception("Not A Valid Link");


            var info = await geoLocTask;
            var (browser, device, os) = AnalyticsModel.ParseUserAgent(context.Request.Headers.UserAgent);

            if (info is { status: "success" })
            {
                db.Analytics.Add(new AnalyticsModel
                {
                    city = info.city,
                    country = info.country,
                    LinkModelCode = code,
                    browser = browser,
                    device = device,
                    os = os,
                    visitedAt = DateTime.Now
                });
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }

        return Results.Redirect(redirectUrl ?? "/");
    });
});
app.Run();