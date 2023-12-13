using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
using Web.Models;
using Web.Models.Account;
using Web.Models.View;
using Web.Models.Link;


namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly UserDbContext _context;

    public HomeController(UserDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Features()
    {
        return View();
    }

    public IActionResult Pricing()
    {
        return View();
    }

    public IActionResult License()
    {
        return View();
    }

    public IActionResult ToS()
    {
        return View();
    }
    
    public IActionResult Login()
    {
        if (HttpContext.Session.Get("UserData") != null)
            return RedirectToAction("Dashboard", "User");
        
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginView data)
    {
        try
        {
            AccountModel? account = _context.Account.FirstOrDefault(acc => acc.email == data.email && acc.password == data.password);
            Console.WriteLine(account);
            if (account != null)
            {
                HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
                _context.ActivityLogs.Add(new ActivityLogModel()
                {
                    Action = account.name + " Logged in with Email.",
                    date = DateTime.Now,
                    IPAddr = "0.0.0.0",
                    Userid = account.id
                });
                _context.SaveChanges();
                return RedirectToAction("Dashboard", "User");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return View();
        }

        return View();
    }

    public IActionResult Signup()
    {
        if (HttpContext.Session.Get("UserData") != null)
            return RedirectToAction("Dashboard", "User");
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Signup(SignupView data)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        try
        {
            _context.Account.Add(new AccountModel()
            {
                email = data.email,
                password = data.password,
                createdAt = DateTime.Now,
                name = data.name
            });
            
            _context.SaveChanges();
            AccountModel? account = _context.Account.FirstOrDefault(acc => acc.email == data.email && acc.password == data.password);
            if (account != null)
            {
                HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
                _context.ActivityLogs.Add(new ActivityLogModel()
                {
                    Action = account.name + " just Signed up with Email.",
                    date = DateTime.Now,
                    IPAddr = "0.0.0.0",
                    Userid = account.id
                });
                _context.SaveChanges();
            }
            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return View();
        }
    }
    
    
}