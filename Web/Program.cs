using Microsoft.EntityFrameworkCore;
using Web;
using Web.ApplicationDbContext;
using Web.Models.Link;

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

app.MapGet("/{code}", (string code, UserDbContext db) =>
{
    Console.Write(code);
    var linkobj  = db.Link.Find(code);
    if (linkobj == null)
    {
        return Results.Redirect("/"); 
    }

    return Results.Redirect(linkobj.url);
});
app.Run();
