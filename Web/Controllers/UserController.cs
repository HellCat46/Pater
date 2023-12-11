using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.Link;
using Web.Models.View.User;

namespace Web.Controllers;

public class UserController : Controller
{
    private readonly UserDbContext _context;

    public UserController(UserDbContext context)
    {
        _context = context;
    }
    public IActionResult Dashboard()
    {
        var sessionId = HttpContext.Session.GetString("SessionID");
        if (sessionId == null)
            return RedirectToAction("Index", "Home");
        
        AccountModel? account = _context.Account.FirstOrDefault(model => model.id == Int32.Parse(sessionId));
        if (account == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        return View(new DashboardView()
        {
            UserName = account.name,
            UserIsAdmin = account.isAdmin,
            UserPicPath = account.PicPath,
            UserPlan = account.Plan,
            links = _context.Link.Where(model => model.AccountId == 1 ).ToList()
        });
    }
    
    public IActionResult Checkout()
    {
        return View();
    }
    public IActionResult Details()
    {
        return View();
    }
    public IActionResult Settings()
    {
        return View();
        
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
    
    
    private string GenerateRandom(int len)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        String code = "";
        for (int i = 0; i < len; i++)
        {
            code += chars[random.Next(1, chars.Length)];
        }

        return code;
    }
}