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
        if (HttpContext.Session.GetString("SessionID") != null)
        {
            return Redirect("User/Dashboard");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Index(IndexView paraIndex)
    {
        String code = GenerateRandom(8);
        try
        {
            _context.Link.Add(new LinkModel()
            {
                code = code,
                name = DateTime.Today.ToString(),
                CreatedAt = DateOnly.FromDateTime(DateTime.Today),
                url = paraIndex.url
            });
            _context.SaveChanges();

            ViewBag.Message = "Created Link : " + code;
            ViewBag.isError = false;
            return View();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex );
            ViewBag.Message = "Failed to create link.";
            ViewBag.isError = true;
            return View();

        }
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginView creds)
    {
        var res= _context.Account.FirstOrDefault(acc => acc.email == creds.email && acc.password == creds.password);
        Console.WriteLine(res);
        if (res != null)
        {
            HttpContext.Session.SetString("SessionID", res.id.ToString());
            return Redirect("User/Dashboard/");
        }

        return View();
    }

    public IActionResult Signup()
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

    public IActionResult InvalidLink()
    {
        return View();
    }
    public string GenerateRandom(int len)
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