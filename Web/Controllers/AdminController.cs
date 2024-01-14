using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            if (bytes == null) return RedirectToAction("Login", "Home");
            AccountModel? adminAccount = AccountModel.Deserialize(bytes);
            if (adminAccount == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            if (adminAccount.isAdmin != true) return RedirectToAction("Dashboard", "User");

            return View(new AdminDashboardView()
            {
                Header = new _HeaderView()
                {
                    isAdmin = adminAccount.isAdmin,
                    name = adminAccount.name,
                    picPath = adminAccount.picPath,
                    plan = adminAccount.plan
                },
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return RedirectToAction("Dashboard", "User");
        }
    }

    public async Task<IActionResult> ManageUser(String? userEmail)
    {
        string? userId = HttpContext.Request.Query["UserId"];

        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null) return RedirectToAction("Login", "Home");
            AccountModel? adminAccount = AccountModel.Deserialize(bytes);
            if (adminAccount == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            
            if (adminAccount.isAdmin != true) return RedirectToAction("Dashboard", "User");

            AccountModel? userAccount;
            if (userEmail != null) userAccount = await context.Account.SingleOrDefaultAsync(acc => acc.email == userEmail);
            else if (userId != null) userAccount = await context.Account.SingleOrDefaultAsync(acc => acc.id == int.Parse(userId));
            else return RedirectToAction("Dashboard");

            if (userAccount == null) return RedirectToAction("Dashboard"); // Replace it proper error message

            return View(new ManageUserView()
            {
                header = new _HeaderView()
                {
                    isAdmin = adminAccount.isAdmin,
                    name = adminAccount.name,
                    picPath = adminAccount.picPath,
                    plan = adminAccount.plan
                },
                UserId = userAccount.id,
                UserAccountCreated = userAccount.createdAt,
                UserEmail = userAccount.email,
                UserName = userAccount.name,
                UserPlan = userAccount.plan,
                UserPicPath = userAccount.picPath
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return View();
    }

    // API Endpoint
    public async Task<IActionResult> GetUserLogs(int pageno, int userId)
    {
        try
        {
            if (pageno < 1)
                return StatusCode(400, new
                {
                    error = "Page Number is too low."
                });

            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });
            AccountModel? adminAccount = AccountModel.Deserialize(bytes);
            if (adminAccount == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            
            if (adminAccount.isAdmin != true)
                return StatusCode(403, new { error = "This action requires Admin Access" });

            return PartialView("_UserLogRows", await context.ActivityLogs.Where(log => log.Userid == userId)
                .Select(log => new UserLog() { date = log.date, action = log.Action })
                .OrderByDescending(log => log.date).Skip((pageno - 1) * 5).Take(5).ToListAsync());            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }
    public async Task<IActionResult> GetLogs(int pageno)
    {
        try
        {
            if (pageno < 1)
                return StatusCode(400, new
                {
                    error = "Page Number is too low."
                });

            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });
            AccountModel? adminAccount = AccountModel.Deserialize(bytes);
            if (adminAccount == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            
            if (adminAccount.isAdmin != true)
                return StatusCode(403, new { error = "This action requires Admin Access" });

            return PartialView("_LogRows", await context.ActivityLogs.OrderByDescending(log => log.date).Skip((pageno - 1) * 10).Take(10).ToListAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }
}