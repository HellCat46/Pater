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
        
        return View(new LinkView()
        {
            message = " "
        });
    }

    [HttpPost]
    public IActionResult Index(LinkView paraLink)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        String code = "";
        for (int i = 0; i < 8; i++)
        {
            code += chars[random.Next(1, chars.Length)];
        }

        try
        {
            _context.Link.Add(new LinkModel()
            {
                code = code,
                CreatedAt = DateOnly.FromDateTime(DateTime.Today),
                url = paraLink.url
            });
            _context.SaveChanges();
            return View(new LinkView()
            {
                created = true,
                message = code,
                url = paraLink.url
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex + "F");
            return View(paraLink);

        }
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
}