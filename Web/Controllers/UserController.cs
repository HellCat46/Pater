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
            links = _context.Link.Where(model => model.AccountId == account.id).ToList()
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
            UserIsAdmin = account.isAdmin
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
    [HttpPost]
    public IActionResult UpdateNameMail(string newName, string newEmail)
    {
        if (newName == null && newEmail == null) return RedirectToAction("Profile");
        
        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            AccountModel account = AccountModel.Deserialize(bytes);

            if (newName != null) account.name = newName;
            if (newEmail != null) account.email = newEmail;

            _context.Account.Update(account);
            _context.SaveChanges();
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            
            if(newName != null) ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.ChangedName, account, "0.0.0.0");
            if(newEmail != null) ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.ChangedEmail, account, "0.0.0.0");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return RedirectToAction("Profile");
    }
    
    [HttpPost]
    public IActionResult CreateLink(CreateLinkView link)
    {
        // if (!ModelState.IsValid)
        // {
        //     return RedirectToAction("Dashboard");
        // }
        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            AccountModel account = AccountModel.Deserialize(bytes);
            _context.Link.Add(new LinkModel()
            {
                AccountId = account.id,
                code = (link.NewLinkCode != null ? link.NewLinkCode : GenerateRandom(8)),
                CreatedAt = DateTime.Now,
                name = (link.NewLinkName != null ? link.NewLinkName : DateTime.Now.ToString()),
                url = link.NewLinkURL
            });
            _context.SaveChanges();
            ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.CreatedLink, account, "0.0.0.0");
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return RedirectToAction("Dashboard");
        }

        ;
    }

    [HttpPost]
    public IActionResult ChangeAvatar(IFormFile file)
    {
        if (file == null)
        {
            return View("Profile");
        }

        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            AccountModel account = AccountModel.Deserialize(bytes);

            if (System.IO.File.Exists( "wwwroot/UserPics/" +account.PicPath))
            {
                System.IO.File.Delete("wwwroot/UserPics/" + account.PicPath);
            }

            account.PicPath = Guid.NewGuid() + "." + file.FileName.Split(".").Last();
            using (var fileStream = System.IO.File.Create("wwwroot/UserPics/" + account.PicPath)){
                file.CopyTo(fileStream);
                fileStream.Close();
            }

            _context.Account.Update(account);
            _context.SaveChanges();
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.ChangedAvatar, account, "0.0.0.0");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return RedirectToAction("Profile");
    }

    [HttpPost]
    public IActionResult ChangePassword(string oldPassword, string newPassword)
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

            if (account.password == oldPassword)
            {
                account.password = newPassword;
                _context.Account.Update(account);
                _context.SaveChanges();
                
                HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
                ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.ChangedPassword, account, "0.0.0.0");
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            
        }

        return Redirect("Profile");
    }

    public IActionResult Logout()
    {
        byte[]? bytes = HttpContext.Session.Get("UserData");
        HttpContext.Session.Clear();
        
        if (bytes == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
        ActivityLogModel.WriteLogs(_context, ActivityLogModel.Event.LoggedOut, AccountModel.Deserialize(bytes), "0.0.0.0");
        
        return RedirectToAction("Index", "Home");
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