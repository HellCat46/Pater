using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.View.User;
using Web.Models.View.Admin;

namespace Web.Controllers;

public class AdminController(UserDbContext context) : Controller
{
    public IActionResult Dashboard()
    {
        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            AccountModel adminAccount = AccountModel.Deserialize(bytes);
            if (adminAccount.isAdmin != true) return RedirectToAction("Dashboard", "User");
            
            return View(new AdminDashboardView()
            {
                Header = new _HeaderView()
                {
                    isAdmin = adminAccount.isAdmin,
                    name = adminAccount.name,
                    picPath = adminAccount.PicPath,
                    plan = adminAccount.Plan
                },
                logs = context.ActivityLogs.OrderByDescending(log => log.date).ToList()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return RedirectToAction("Dashboard", "User");
        }
    }

    public IActionResult ManageUser(String userEmail)
    {
        string? userId = HttpContext.Request.Query["UserId"];
        
        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }

            AccountModel adminAccount = AccountModel.Deserialize(bytes);
            if (adminAccount.isAdmin != true) return RedirectToAction("Dashboard", "User");

            AccountModel? userAccount;
            if(userEmail != null) userAccount = context.Account.Single(acc => acc.email == userEmail);
            else if (userId != null ) userAccount = context.Account.Single(acc => acc.id == int.Parse(userId));
            else return RedirectToAction("Dashboard");
            
            
            return View(new ManageUserView()
            {
                header = new _HeaderView()
                {
                    isAdmin = adminAccount.isAdmin,
                    name = adminAccount.name,
                    picPath = adminAccount.PicPath,
                    plan = adminAccount.Plan
                },
                links = context.Link.Where(link => link.AccountId == userAccount.id).ToList(),
                logs = context.ActivityLogs.Where(log => log.Userid == userAccount.id).ToList(),
                UserAccountCreated = userAccount.createdAt,
                UserEmail = userAccount.email,
                UserName = userAccount.name,
                UserPlan = userAccount.Plan,
                UserPicPath = userAccount.PicPath
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return View();
    }
}