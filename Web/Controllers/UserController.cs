using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
using Web.Models;
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
        byte[]? bytes = HttpContext.Session.Get("UserData");
        if (bytes == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
        AccountModel account = AccountModel.Deserialize(bytes);
        return View(new DashboardView()
        {
            UserName = account.name,
            UserIsAdmin = account.isAdmin,
            UserPicPath = account.PicPath,
            UserPlan = account.Plan,
            links = _context.Link.Where(model => model.AccountId == account.id ).ToList()
        });
    }
    
    public IActionResult Profile()
    {
        byte[]? bytes = HttpContext.Session.Get("UserData");
        if (bytes == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
        AccountModel account = AccountModel.Deserialize(bytes);
        return View(new ProfileView()
        {
            UserName = account.name,
            UserEmail = account.email,
            UserPicPath = account.PicPath,
            UserPlan = account.Plan,
            UserIsAdmin = account.isAdmin,
            Changes = false
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

    
    
    // Non-Page Actions
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public IActionResult CreateLink(CreateLinkView link)
    {
        if (!ModelState.IsValid)
        {
            return View("Dashboard");
        }
    
        byte[]? bytes = HttpContext.Session.Get("UserData");
        if (bytes == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
        AccountModel account = AccountModel.Deserialize(bytes);
        try
        {
            _context.Link.Add(new LinkModel()
            {
                AccountId = account.id,
                code = (link.NewLinkCode != null ? link.NewLinkCode : GenerateRandom(8)),
                CreatedAt = DateTime.Now,
                name = (link.NewLinkName != null ? link.NewLinkName : DateTime.Now.ToString()),
                url = link.NewLinkURL
            });
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return RedirectToAction("Dashboard");
        };
    }
    
    [HttpPost]
    public IActionResult ChangePassword()
    {
        return Redirect("Profile");
    }

    public IActionResult DeleteAccount()
    {
        byte[]? bytes = HttpContext.Session.Get("UserData");
        if (bytes == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
        AccountModel account = AccountModel.Deserialize(bytes);
        try
        {
            _context.Account.Remove(_context.Account.Single(acc => acc.id == account.id));

            var links = _context.Link.Where(link => link.AccountId == account.id);
            foreach (LinkModel link in links)
            {
                _context.Link.Remove(link);
            }
            var logs = _context.ActivityLogs.Where(logs => logs.Userid == account.id);
            foreach (ActivityLogModel log in logs)
            {
                _context.ActivityLogs.Remove(log);
            }
            _context.SaveChanges();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return RedirectToAction("Profile");
        }
    }
    // Non-Action Functions
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