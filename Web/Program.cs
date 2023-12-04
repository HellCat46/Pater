using Microsoft.EntityFrameworkCore;
using Web.ApplicationDbContext;
using Web.Models.Link;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("prod")));

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
