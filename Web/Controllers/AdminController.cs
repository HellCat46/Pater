using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Web.ApplicationDbContext;
using Web.Models;
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
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Unexpected Error while processing the request."
            });
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
            if (userEmail != null) userAccount = await context.Account.Include(acc => acc.ExternalAuth).SingleOrDefaultAsync(acc => acc.email == userEmail);
            else if (userId != null) userAccount = await context.Account.Include(acc => acc.ExternalAuth).SingleOrDefaultAsync(acc => acc.id == int.Parse(userId));
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
                UserPicPath = userAccount.picPath,
                UserAccountCreated = userAccount.createdAt.ToString("yyyy'-'MM'-'dd'T'HH':'mm"),
                UserAuthMethod = userAccount.ExternalAuth == null ? "Email" : userAccount.ExternalAuth.Provider.ToString(),
                UserId = userAccount.id,
                UserOAuthId = userAccount.ExternalAuth?.UserID,
                UserEmail = userAccount.email,
                UserName = userAccount.name,
                UserPlan = userAccount.plan,
                UserLinkLimit = userAccount.linkLimit,
                UserIsVerified = userAccount.isVerified,
                plans = Enum.GetNames(typeof(AccountModel.Plan))
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Unexpected Error while processing the request."
            });
        }
    }

    // Non-Action Endpoints
    public async Task<IActionResult> GetUserLogs(int pageNo, int userId)
    {
        try
        {
            if (pageNo < 1)
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
                .OrderByDescending(log => log.date).Skip((pageNo - 1) * 5).Take(5).ToListAsync());            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateUser(UpdateUser data)
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

            if (!adminAccount.isAdmin) return StatusCode(403, new { error = "This action requires Admin Access." });

            var userAccount = await context.Account.Include(acc => acc.ExternalAuth).FirstOrDefaultAsync(acc => acc.id == data.UserId);
            if (userAccount == null)
                return StatusCode(404, new { error = "User no longer exists"});
            if (userAccount.ExternalAuth != null && data.Password != null)
                return StatusCode(400, new { error = "Password of Google User can't be changed." });

            if (data.Email != null) userAccount.email = data.Email;
            if (data.Name != null) userAccount.name = data.Name;
            if (data.Password != null) userAccount.password = data.Password;
            if (data.Plan != null) userAccount.plan = data.Plan.Value;
            if (data.Verified != null) userAccount.isVerified = data.Verified.Value;
            if (data.LinkLimit != null) userAccount.linkLimit = data.LinkLimit.Value;

            await context.SaveChangesAsync();
            
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }
    
    public async Task<IActionResult> GetLogs(int pageNo)
    {
        try
        {
            if (pageNo < 1)
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

            return PartialView("_LogRows", await context.ActivityLogs.OrderByDescending(log => log.date).Skip((pageNo - 1) * 10).Take(10).ToListAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }

}