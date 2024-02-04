using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ApplicationDbContext;
using Web.Controllers;
using Web.Models;
using Web.Models.Account;
using Web.Models.View.User;

namespace Web.Areas.User.Controllers;

[Area("User")]
public class UserController(IConfiguration config, UserDbContext context) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var sessionAcc = SessionAccountModel.GetSession(HttpContext);
        if (sessionAcc == null) return RedirectToAction("Login", "Home");


        if (!sessionAcc.isVerified && sessionAcc.createdAt <= DateTime.Now.Subtract(TimeSpan.FromDays(7)))
            return View("UnVerified");
        if (!sessionAcc.isVerified) ViewData["UnVerified"] = 1;

        return View(new DashboardView
        {
            header = new _HeaderView
            {
                isAdmin = sessionAcc.isAdmin,
                name = sessionAcc.name,
                picPath = sessionAcc.picPath,
                isPaidUser = sessionAcc.isPaidUser
            },
            hasLinks = await context.Link.AnyAsync(link => link.AccountId == sessionAcc.id)
        });
    }

    public IActionResult Profile()
    {
        var sessionAcc = SessionAccountModel.GetSession(HttpContext);
        if (sessionAcc == null) return RedirectToAction("Login", "Home");


        if (!sessionAcc.isVerified && sessionAcc.createdAt <= DateTime.Now.Subtract(TimeSpan.FromDays(7)))
            return View("UnVerified");
        if (!sessionAcc.isVerified) ViewData["UnVerified"] = 1;

        var email = context.Account.Where(acc => acc.id == sessionAcc.id).Select(acc => acc.email).FirstOrDefault();
        if (email == null)
            return View("ErrorPage", new ErrorView
            {
                errorCode = 404,
                errorTitle = "Essential Data Missing",
                errorMessage = "It seems your account doesn't have any email linked to it. Please contact support."
            });

        return View(new ProfileView
        {
            header = new _HeaderView
            {
                isAdmin = sessionAcc.isAdmin,
                name = sessionAcc.name,
                picPath = sessionAcc.picPath,
                isPaidUser = sessionAcc.isPaidUser
            },
            UserName = sessionAcc.name,
            UserEmail = email
        });
    }

    public IActionResult Checkout()
    {
        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return RedirectToAction("Login", "Home");


            return View(new CheckoutView
            {
                header = new _HeaderView
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

    public async Task<IActionResult> Details(string? code)
    {
        try
        {
            if (code == null) RedirectToAction("Dashboard");


            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return RedirectToAction("Login", "Home");

            if (!sessionAcc.isVerified && sessionAcc.createdAt <= DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                return View("UnVerified");
            if (!sessionAcc.isVerified) ViewData["UnVerified"] = 1;

            var linkDetails =
                await context.Link.Include(li => li.Account)
                    .FirstOrDefaultAsync(link => link.code == code && link.AccountId == sessionAcc.id);
            if (linkDetails == null) return RedirectToAction("Dashboard");

            return View(new DetailsView
            {
                header = new _HeaderView
                {
                    isAdmin = sessionAcc.isAdmin,
                    name = sessionAcc.name,
                    picPath = sessionAcc.picPath,
                    isPaidUser = sessionAcc.isPaidUser
                },
                linkDetails = linkDetails,
                userPlan = linkDetails.Account.plan
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

    public IActionResult VerificationRequest()
    {
        try
        {
            var mailConfig = config.GetSection("MailServer").Get<MailServer>();
            if (mailConfig == null)
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 500,
                    errorTitle = "Server Error",
                    errorMessage = "Please contact support through email."
                });

            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return RedirectToAction("Login", "Home");
            if (sessionAcc.isVerified) return RedirectToAction("Dashboard");

            var email = context.Account.Where(acc => acc.id == sessionAcc.id).Select(acc => acc.email).FirstOrDefault();
            if (email == null)
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 404,
                    errorTitle = "Essential Data Missing",
                    errorMessage = "It seems your account doesn't have any email linked to it. Please contact support."
                });

            var dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            if (context.AuthAction.Any(au =>
                    au.Userid == sessionAcc.id && au.action == AuthActionModel.ActionType.VerifyEmail &&
                    au.createAt >= dateTime))
                return View();

            var code = HomeController.GenerateRandom(50);
            context.AuthAction.Add(new AuthActionModel
            {
                action = AuthActionModel.ActionType.VerifyEmail,
                code = code,
                createAt = DateTime.Now,
                Userid = sessionAcc.id
            });
            context.SaveChanges();

            var link = Url.ActionLink("VerifyMail", "Home") + "?code=" + code;
            MailingSystem.SendEmailVerification(mailConfig, sessionAcc.name, email, link);
            return View();
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