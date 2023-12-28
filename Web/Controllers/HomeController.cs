using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.View;


namespace Web.Controllers;

public class HomeController(UserDbContext context) : Controller
{
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
            AccountModel? account = context.Account.FirstOrDefault(acc => acc.email == data.email && acc.password == data.password);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Invalid Credentials";
                return View();
            }
            if (account.password != data.password)
            {
                ViewBag.ErrorMessage = ".";
                return View();
            }
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.EmailLoggedIn, account, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            Console.Write(account.ToString());
            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ViewBag.ErrorMessage = "Unexpected Error while processing the request";
            return View();
        }
    }

    public IActionResult Signup()
    {
        if (HttpContext.Session.Get("UserData") != null)
            return RedirectToAction("Dashboard", "User");
        
        return View();
    }

    [HttpPost]
    public IActionResult Signup(SignupView data)
    {
        try
        {
            AccountModel? account = context.Account.FirstOrDefault(acc => acc.email == data.email);
            if (account != null)
            {
                ViewBag.ErrorMessage = "Account Already Exist with this Email";
                return View();
            }

            context.Account.Add(new AccountModel()
            {
                email = data.email,
                password = data.password,
                createdAt = DateTime.Now,
                name = data.name
            });
            context.SaveChanges();
            
            account = context.Account.FirstOrDefault(acc => acc.email == data.email && acc.password == data.password);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Failed to create the Account. Please try again later.";
                return View();
            }
            
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.EmailSignedIn, account, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            
            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ViewBag.ErrorMessage = "Unexpected Error while processing the request";
            return View();
        }
    }
    
    
}