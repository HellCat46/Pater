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
            ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.EmailLoggedIn, account, HttpContext.Connection.RemoteIpAddress.ToString());
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
            AccountModel? account = _context.Account.FirstOrDefault(acc => acc.email == data.email);
            if (account != null)
            {
                ViewBag.ErrorMessage = "Account Already Exist with this Email";
                return View();
            }

            _context.Account.Add(new AccountModel()
            {
                email = data.email,
                password = data.password,
                createdAt = DateTime.Now,
                name = data.name
            });
            _context.SaveChanges();
            
            account = _context.Account.FirstOrDefault(acc => acc.email == data.email && acc.password == data.password);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Failed to create the Account. Please try again later.";
                return View();
            }
            
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.EmailSignedIn, account, HttpContext.Connection.RemoteIpAddress.ToString());
            
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