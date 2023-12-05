using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
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
        
        return View(new IndexView()
        {
            message = " "
        });
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
                CreatedAt = DateOnly.FromDateTime(DateTime.Today),
                url = paraIndex.url
            });
            _context.SaveChanges();
            return View(new IndexView()
            {
                created = true,
                message = code,
                url = paraIndex.url
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex + "F");
            return View(paraIndex);

        }
    }

    public IActionResult Login()
    {
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