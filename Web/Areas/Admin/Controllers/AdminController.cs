using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ApplicationDbContext;
using Web.Models;
using Web.Models.Account;
using Web.Models.View.Admin;
using Web.Models.View.User;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminController(UserDbContext context) : Controller
{
    public IActionResult Dashboard()
    {
        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return RedirectToAction("Login", "Home", new {area = ""});
            if (sessionAcc.isAdmin != true) return RedirectToAction("Dashboard", "User", new {area = "User"});

            return View(new AdminDashboardView
            {
                Header = new _HeaderView
                {
                    isAdmin = sessionAcc.isAdmin,
                    name = sessionAcc.name,
                    picPath = sessionAcc.picPath,
                    isPaidUser = sessionAcc.isPaidUser
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return View("ErrorPage", new ErrorView
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Unexpected Error while processing the request."
            });
        }
    }

    public async Task<IActionResult> ManageUser(string? userEmail)
    {
        string? userId = HttpContext.Request.Query["UserId"];

        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return RedirectToAction("Login", "Home", new {area = ""});

            if (sessionAcc.isAdmin != true) return RedirectToAction("Dashboard", "User", new {area = "User"});

            AccountModel? userAccount;
            if (userEmail != null)
                userAccount = await context.Account.Include(acc => acc.ExternalAuth)
                    .SingleOrDefaultAsync(acc => acc.email == userEmail);
            else if (userId != null)
                userAccount = await context.Account.Include(acc => acc.ExternalAuth)
                    .SingleOrDefaultAsync(acc => acc.id == int.Parse(userId));
            else return RedirectToAction("Dashboard");

            if (userAccount == null) return RedirectToAction("Dashboard"); // Replace it proper error message

            return View(new ManageUserView
            {
                header = new _HeaderView
                {
                    isAdmin = sessionAcc.isAdmin,
                    name = sessionAcc.name,
                    picPath = sessionAcc.picPath,
                    isPaidUser = sessionAcc.isPaidUser
                },
                UserPicPath = userAccount.picPath,
                UserAccountCreated = userAccount.createdAt.ToString("yyyy'-'MM'-'dd'T'HH':'mm"),
                UserAuthMethod = userAccount.ExternalAuth == null
                    ? "Email"
                    : userAccount.ExternalAuth.Provider.ToString(),
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
            return View("ErrorPage", new ErrorView
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Unexpected Error while processing the request."
            });
        }
    }

}