using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.View.User;
using Web.Models.View.Admin;

namespace Web.Controllers;

public class AdminController : Controller
{
    private readonly UserDbContext _context;
    public AdminController(UserDbContext context)
    {
        _context = context;
    }
    
    public IActionResult Dashboard()
    {
        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            AccountModel account = AccountModel.Deserialize(bytes);
            if (account.isAdmin != true) return RedirectToAction("Dashboard", "User");
            
            return View(new AdminDashboardView()
            {
                Header = new _HeaderView()
                {
                    isAdmin = account.isAdmin,
                    name = account.name,
                    picPath = account.PicPath,
                    plan = account.Plan
                },
                logs = _context.ActivityLogs.OrderByDescending(log => log.date).ToList()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return RedirectToAction("Dashboard", "User");
        }
    }
}