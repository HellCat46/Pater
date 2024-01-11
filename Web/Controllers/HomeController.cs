using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.View;


namespace Web.Controllers;

public class HomeController(IConfiguration config, UserDbContext context) : Controller
{
    public IActionResult Index()
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
    
    public IActionResult Login()
    {
        if (HttpContext.Session.Get("UserData") != null)
            return RedirectToAction("Dashboard", "User");
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginView data)
    {
        try
        {
            AccountModel? account = await context.Account.FirstOrDefaultAsync(acc => acc.email == data.email && acc.password == data.password);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Invalid Credentials";
                return View();
            }
            if (account.password != data.password)
            {
                ViewBag.ErrorMessage = ".";
                return View();
            }
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.EmailLoggedIn, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ViewBag.ErrorMessage = "Unexpected Error while processing the request";
            return View();
        }
    }

    public IActionResult Signup()
    {
        if (HttpContext.Session.Get("UserData") != null)
            return RedirectToAction("Dashboard", "User");
        
        return View();
    }
    
    [Route("/ResetPassword")]
    public IActionResult ResetPassword(string code)
    {
        
        DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
        if (context.AuthAction.Any(au => au.code == code && au.createAt >= dateTime)) return View();
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Route("/ResetPassword")]
    public IActionResult SetPassword(string code, [FromBody] ResetPassword info)
    {
        try
        {
            DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            var action = context.AuthAction.FirstOrDefault(au => au.code == code && au.createAt >= dateTime);
            if (action == null)
            {
                return StatusCode(404, new
                {
                    error = "Link is either Expired or Invalid."
                });
            }

            var account = context.Account.FirstOrDefault(acc => acc.id == action.Userid);
            if (account == null)
            {
                return StatusCode(404, new
                {
                    error = "Account No Longer Exists."
                });
            }

            account.password = info.password;
            context.SaveChanges();
            
            return Ok();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while processing the request"
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupView data)
    {
        try
        {
            AccountModel? account = await context.Account.FirstOrDefaultAsync(acc => acc.email == data.email);
            if (account != null)
            {
                ViewBag.ErrorMessage = "Account Already Exist with this Email";
                return View();
            }

            await context.Account.AddAsync(new AccountModel()
            {
                email = data.email,
                password = data.password,
                createdAt = DateTime.Now,
                name = data.name
            });
            await context.SaveChangesAsync();
            
            account = await context.Account.FirstOrDefaultAsync(acc => acc.email == data.email && acc.password == data.password);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Failed to create the Account. Please try again later.";
                return View();
            }
            
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.EmailSignedIn, account, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            
            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ViewBag.ErrorMessage = "Unexpected Error while processing the request";
            return View();
        }
    }
    
    
    // Non-Action Methods
    [HttpPost]
    public IActionResult ResetPasswordRequest(ResetPasswordReq info)
    {
        try
        {
            MailServer? mailConfig = config.GetSection("MailServer").Get<MailServer>();
            if (mailConfig == null)
            {
                return StatusCode(500, new { error = "Server Error. Please contact support through email." });
            }
            
            var acc = context.Account.Where(acc => acc.email == info.email).Select(acc => new {Accountid = acc.id, AccountName = acc.name}).ToList();
            if (acc.Count() == 0)
            {
                return NotFound(new
                {
                    error = "No Account Associated with this email."
                });
            }

            DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            if (context.AuthAction.Any(au =>
                    au.Userid == acc[0].Accountid && au.createAt >=dateTime ))
            {
                return StatusCode(409, new
                {
                    error = "Link has already been sent to your email."
                });
            }
            
            String code= UserController.GenerateRandom(50);
            context.AuthAction.Add(new AuthActionModel()
            {
                action = AuthActionModel.ActionType.ResetPassword,
                code = code,
                createAt = DateTime.Now,
                Userid = acc[0].Accountid
            });
            context.SaveChanges();

            string link = HttpContext.Request.Headers.Origin + Url.Action("ResetPassword") + "?code=" + code;
            MailingSystem.SendPasswordReset(mailConfig, acc[0].AccountName, info.email, link);
            
            return Ok();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while processing the request"
            });
        }
    }
    
}

public class ResetPasswordReq
{
    public string email {get; set; }
}

public class ResetPassword
{
    public string password { get; set; }
}