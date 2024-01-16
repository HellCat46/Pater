using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.View.Home;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using Web.Models;

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
            AccountModel? account =
                await context.Account.FirstOrDefaultAsync(acc =>
                    acc.email == data.email && acc.password == data.password);
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

    [Route("/GoogleAuthRedirect")]
    public IActionResult GoogleAuthRedirect()
    {
        GoogleOAuth? auth = config.GetSection("GoogleOAuth").Get<GoogleOAuth>();
        if (auth == null)
        {
            return StatusCode(500, new { error = "Server Error. Please contact support through email." });
        }

        string url = String.Format(
            "https://accounts.google.com/o/oauth2/v2/auth?scope=openid https%3A//www.googleapis.com/auth/userinfo.email https%3A//www.googleapis.com/auth/userinfo.profile&access_type=online&include_granted_scopes=true&response_type=code&state=state_parameter_passthrough_value&redirect_uri={0}&client_id={1}",
            Url.ActionLink("GoogleAuth"), auth.ClientId);
        
        return Redirect(url);
    }

    [Route("/GoogleAuth")]
    public IActionResult GoogleAuth(string code)
    {
        if (code.IsNullOrEmpty())
        {
            return StatusCode(400, new { error = "Required Parameters are missing" });
        }

        GoogleOAuth? auth = config.GetSection("GoogleOAuth").Get<GoogleOAuth>();
        if (auth == null)
        {
            return StatusCode(500, new { error = "Server Error. Please contact support through email." });
        }


        try
        {
            GoogleJsonWebSignature.Payload? payload;
            using (HttpClient client = new HttpClient())
            {
                Uri url = new Uri(String.Format(
                    "https://oauth2.googleapis.com/token?code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
                    code, auth.ClientId, auth.ClientSecret, "http://localhost:3000/GoogleAuth"));
                Console.WriteLine(url.ToString());
                var res = client.PostAsync(url, null).Result; //<TokenResponse>(url).Result;
                var tokenRes = res.Content.ReadFromJsonAsync<TokenResponse>().Result;
                if (tokenRes == null)
                    return StatusCode(500, new { error = "Errors while trying to validate the code" });

                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { auth.ClientId }
                };
                payload = GoogleJsonWebSignature.ValidateAsync(tokenRes.id_token, settings).Result;
            }

            if (payload == null)
                return StatusCode(500, new { error = "Unable to get user info from Google. Please try again" });

            var exAuth = context.ExternalAuth.FirstOrDefault(ea => ea.UserID == payload.Subject);
            if (exAuth != null)
            {
                var acc = context.Account.FirstOrDefault(acc => acc.id == exAuth.AccountId);
                if (acc == null)
                {
                    context.ExternalAuth.Remove(exAuth);
                    context.SaveChanges();
                    return RedirectToAction("Signup");
                }

                acc.ExternalAuth = null;
                HttpContext.Session.Set("UserData", AccountModel.Serialize(acc));
                return RedirectToAction("Dashboard", "User");
            }

            var exAuthEntityEntry = context.ExternalAuth.Add(new ExternalAuthModel()
            {
                Account = new AccountModel()
                {
                    createdAt = DateTime.Now,
                    email = payload.Email,
                    isAdmin = false,
                    isVerified = true,
                    linkLimit = 5,
                    name = payload.Name,
                    plan = AccountModel.Plan.Free
                },
                Provider = ExternalAuthModel.AuthProvider.Google,
                UserID = payload.Subject
            });
            context.SaveChanges();
            
            var account = context.Account.FirstOrDefault(acc => acc.id == exAuthEntityEntry.Entity.AccountId);
            if (account == null)
            {
                context.ExternalAuth.Remove(exAuthEntityEntry.Entity);
                context.SaveChanges();
                return RedirectToAction("Signup");
            }

            
            account.ExternalAuth = null;
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while processing the request."
            });
        }
    }


    public IActionResult Signup()
    {
        if (HttpContext.Session.Get("UserData") != null)
            return RedirectToAction("Dashboard", "User");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupView data)
    {
        try
        {
            MailServer? mailConfig = config.GetSection("MailServer").Get<MailServer>();
            if (mailConfig == null)
            {
                return StatusCode(500, new { error = "Server Error. Please contact support through email." });
            }

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
                name = data.name,
                linkLimit = 5
            });
            await context.SaveChangesAsync();

            account = await context.Account.FirstOrDefaultAsync(acc =>
                acc.email == data.email && acc.password == data.password);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Failed to create the Account. Please try again later.";
                return View();
            }

            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.EmailSignedIn, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            MailingSystem.SendWelcomeMail(mailConfig, account.name, account.email);

            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ViewBag.ErrorMessage = "Unexpected Error while processing the request";
            return View();
        }
    }


    [Route("/ResetPassword")]
    public IActionResult ResetPassword(string code)
    {
        try
        {
            DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            if (context.AuthAction.Any(au =>
                    au.code == code && au.action == AuthActionModel.ActionType.ResetPassword &&
                    au.createAt >= dateTime))
                return View();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return RedirectToAction("Index");
    }


    [HttpPost]
    [Route("/ResetPassword")]
    public IActionResult SetPassword(string code, [FromBody] ResetPassword info)
    {
        try
        {
            DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            var action = context.AuthAction.FirstOrDefault(au =>
                au.code == code && au.action == AuthActionModel.ActionType.ResetPassword && au.createAt >= dateTime);
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
            context.AuthAction.Remove(action);
            context.SaveChanges();

            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ResetPassword, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while processing the request"
            });
        }
    }

    [Route("/VerifyMail")]
    public IActionResult VerifyMail(string code)
    {
        try
        {
            DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            var action = context.AuthAction.FirstOrDefault(au =>
                au.code == code && au.action == AuthActionModel.ActionType.VerifyEmail && au.createAt >= dateTime);
            if (action == null) return RedirectToAction("Index");

            var acc = context.Account.FirstOrDefault(acc => acc.id == action.Userid);
            if (acc == null)
            {
                return RedirectToAction("Index");
            }

            acc.isVerified = true;
            context.AuthAction.Remove(action);
            context.SaveChanges();

            if (HttpContext.Session.Get("UserData") != null)
            {
                HttpContext.Session.Set("UserData", AccountModel.Serialize(acc));
            }

            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.VerifyEmail, acc,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return View("VerifyEmail");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return RedirectToAction("Index");
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

            var acc = context.Account.Where(acc => acc.email == info.email)
                .Select(acc => new { Accountid = acc.id, AccountName = acc.name }).ToList();
            if (acc.Count() == 0)
            {
                return NotFound(new
                {
                    error = "No Account Associated with this email."
                });
            }

            DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            if (context.AuthAction.Any(au =>
                    au.Userid == acc[0].Accountid && au.action == AuthActionModel.ActionType.ResetPassword &&
                    au.createAt >= dateTime))
            {
                return StatusCode(409, new
                {
                    error = "Link has already been sent to your email."
                });
            }

            String code = UserController.GenerateRandom(50);
            context.AuthAction.Add(new AuthActionModel()
            {
                action = AuthActionModel.ActionType.ResetPassword,
                code = code,
                createAt = DateTime.Now,
                Userid = acc[0].Accountid
            });
            context.SaveChanges();

            string link = Url.ActionLink("ResetPassword") + "?code=" + code;
            MailingSystem.SendPasswordReset(mailConfig, acc[0].AccountName, info.email, link);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while processing the request"
            });
        }
    }
}

public class ResetPasswordReq
{
    public string email { get; set; }
}

public class ResetPassword
{
    public string password { get; set; }
}