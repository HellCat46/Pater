using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
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
        if (HttpContext.Session.GetString("SessionID") != null)
            return RedirectToAction("Dashboard", "User");
        
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginView data)
    {
        var res= _context.Account.FirstOrDefault(acc => acc.email == data.email && acc.password == data.password);
        Console.WriteLine(res);
        if (res != null)
        {
            HttpContext.Session.SetString("SessionID", res.id.ToString());
            return RedirectToAction("Dashboard", "User");
        }
    
        return View();
    }

    public IActionResult Signup()
    {
        if (HttpContext.Session.GetString("SessionID") != null)
            return RedirectToAction("Dashboard", "User");
        
        return View();
    }

    [HttpPost]
    public IActionResult Signup(SignupView data)
    { 
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
            var acc = _context.Account.FirstOrDefault(acc => acc.email == data.email && acc.password == data.password);
            if (acc != null)
            {
                HttpContext.Session.SetString("SessionID", acc.id.ToString());
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