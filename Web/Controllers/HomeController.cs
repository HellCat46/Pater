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
            if (data.password.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "Password field is required.";
                return View();
            } 
                
            AccountModel? account =
                await context.Account.FirstOrDefaultAsync(acc =>
                    acc.email == data.email && acc.password == data.password);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Invalid Credentials";
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
        string state = UserController.GenerateRandom(10);
        HttpContext.Session.SetString("GoogleState", state);
            
        GoogleOAuth? auth = config.GetSection("GoogleOAuth").Get<GoogleOAuth>();
        if (auth == null)
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Please contact support through email."
            });

        string url = String.Format(
            "https://accounts.google.com/o/oauth2/v2/auth?scope=openid https%3A//www.googleapis.com/auth/userinfo.email https%3A//www.googleapis.com/auth/userinfo.profile&access_type=online&include_granted_scopes=true&response_type=code&state={0}&redirect_uri={1}&client_id={2}", 
            state, Url.ActionLink("GoogleAuth"), auth.ClientId);
        
        return Redirect(url);
    }

    [Route("/GoogleAuth")]
    public IActionResult GoogleAuth(string? error, string state, string code)
    {
        if (!error.IsNullOrEmpty())
        {
            var errorView = new ErrorView() {errorCode = 400};
            switch (error)
            {
                case "admin_policy_enforced" :
                    errorView.errorTitle = "Not Enough Scopes";
                    errorView.errorMessage = "Unable to get required user information. Please contact your Google Workspace Admin";
                    break;
                case "disallowed_useragent" :
                    errorView.errorTitle = "DisAllowed";
                    errorView.errorMessage = "Google doesn't allow authorization with this source. Try with some other browser or device";
                    break;
                case "org_internal" :
                    errorView.errorTitle = "Development Mode";
                    errorView.errorMessage = "Please contact support team to inform them about this issue.";
                    break;
                case "invalid_client" :
                    errorView.errorTitle = "MisConfigured Client";
                    errorView.errorMessage = "Please contact support team to inform them about this issue";
                    break;
                case "invalid_grant" :
                    errorView.errorTitle = "Token expired";
                    errorView.errorMessage = "Please Try again";
                    break;
                case "redirect_uri_mismatch" :
                    errorView.errorTitle = "MisConfigured Redirect Url";
                    errorView.errorMessage = "Please contact support team to inform them about this issue";
                    break;
                case "invalid_request" :
                    errorView.errorTitle = "Invalid Request";
                    errorView.errorMessage = "Please try again and if you are facing same issue then contact support.";
                    break;
            }

            return View("ErrorPage", errorView);
        }
        if (code.IsNullOrEmpty() || state.IsNullOrEmpty())
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 400,
                errorTitle = "Bad Request",
                errorMessage = "Required Parameters are missing."
            });
        
        
        string? stateId = HttpContext.Session.GetString("GoogleState");
        if (stateId == null)
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 400,
                errorTitle = "Bad Request",
                errorMessage = "No State was found. Please Try Again."
            });
        if (state != stateId)
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 403,
                errorTitle = "Forbidden",
                errorMessage = "The Request looks Suspicious."
            });
        HttpContext.Session.Remove("GoogleState");
        
        
        MailServer? mailConfig = config.GetSection("MailServer").Get<MailServer>();
        GoogleOAuth? auth = config.GetSection("GoogleOAuth").Get<GoogleOAuth>();
        if (auth == null || mailConfig == null)
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Please contact support through Email."
            });


        try
        {
            GoogleJsonWebSignature.Payload? payload;
            using (HttpClient client = new HttpClient())
            {
                Uri url = new Uri(String.Format(
                    "https://oauth2.googleapis.com/token?code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
                    code, auth.ClientId, auth.ClientSecret, "http://localhost:3000/GoogleAuth"));
                var res = client.PostAsync(url, null).Result; //<TokenResponse>(url).Result;
                var tokenRes = res.Content.ReadFromJsonAsync<TokenResponse>().Result;
                if (tokenRes == null)
                    return View("ErrorPage", new ErrorView()
                    {
                        errorCode = 500,
                        errorTitle = "Server Error",
                        errorMessage = "Errors while trying to validate the code from Google."
                    });

                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { auth.ClientId }
                };
                payload = GoogleJsonWebSignature.ValidateAsync(tokenRes.id_token, settings).Result;
            }
            if (payload == null)
                return View("ErrorPage", new ErrorView()
                {
                    errorCode = 500,
                    errorTitle = "Server Error",
                    errorMessage = "Unable to get user info from Google. Please try again"
                });
            
            var exAuth = context.ExternalAuth.Include(ea => ea.Account).FirstOrDefault(ea => ea.UserID == payload.Subject);
            if (exAuth != null)
            {
                var acc = exAuth.Account;
                HttpContext.Session.Set("UserData", AccountModel.Serialize(new AccountModel()
                {
                    id = acc.id,
                    email = acc.email,
                    isAdmin = acc.isAdmin,
                    isVerified = acc.isVerified,
                    plan = acc.plan,
                    linkLimit = acc.linkLimit,
                    createdAt = acc.createdAt,
                    name = acc.name,
                    password = acc.password,
                    picPath = acc.picPath
                }));
                ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.GoogleLoggedIn, acc,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
                
                return RedirectToAction("Dashboard", "User");
            }

            if (context.Account.Any(acc => acc.email == payload.Email))
                return View("ErrorPage", new ErrorView()
                {
                    errorCode = 409,
                    errorTitle = "Email Conflict",
                    errorMessage = "Account Already Exists with your account email. Either Use that account or Try with different Google Account."
                });

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

            var account = exAuthEntityEntry.Entity.Account;
            
            try
            {
                using (var client = new HttpClient())
                {
                    var res = client.GetAsync(payload.Picture).Result;
                    string fileName = account.id +"."+res.Content.Headers.GetValues("Content-Disposition").ToList()[0].Split(";").Last().Split("=").Last().Trim('"').Split(".").Last();
                    using (var fileStream = System.IO.File.Create("wwwroot/UserPics/"+fileName))
                    {
                        var bytes = res.Content.ReadAsByteArrayAsync().Result;
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Close();
                    }

                    account.picPath = fileName;
                    context.SaveChanges();
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            HttpContext.Session.Set("UserData", AccountModel.Serialize(new AccountModel()
            {
                id = account.id,
                email = account.email,
                isAdmin = account.isAdmin,
                isVerified = account.isVerified,
                plan = account.plan,
                linkLimit = account.linkLimit,
                createdAt = account.createdAt,
                name = account.name,
                password = account.password,
                picPath = account.picPath
            }));
            
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.GoogleSignedUp, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            try
            {
                MailingSystem.SendWelcomeMail(mailConfig, account.name, account.email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return RedirectToAction("Dashboard", "User");
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
                return View("ErrorPage", new ErrorView()
                {
                    errorCode = 500,
                    errorTitle = "Server Error",
                    errorMessage = "Please contact support through email."
                });

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
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.EmailSignedUp, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            try
            {
                MailingSystem.SendWelcomeMail(mailConfig, account.name, account.email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            return RedirectToAction("Dashboard", "User");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ViewBag.ErrorMessage = "Unexpected Error while processing the request.";
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
            return View("ErrorPage", new ErrorView()
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Unexpected Error while processing the request."
            });
        }

        return RedirectToAction("Index");
    }
    

    [Route("/VerifyMail")]
    public IActionResult VerifyMail(string code)
    {
        try
        {
            DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
            var action = context.AuthAction.Include(au =>au.User).FirstOrDefault(au =>
                au.code == code && au.action == AuthActionModel.ActionType.VerifyEmail && au.createAt >= dateTime);
            if (action == null) return RedirectToAction("Index");
            
            action.User.isVerified = true;
            context.AuthAction.Remove(action);
            context.SaveChanges();

            if (HttpContext.Session.Get("UserData") != null)
            {
                HttpContext.Session.Set("UserData", AccountModel.Serialize(action.User));
            }

            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.VerifyEmail, action.User,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return View("VerifyEmail");
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
                return NotFound(new {
                    error = "No Account Associated with this email."
                });
            if(context.ExternalAuth.Any(ea => ea.AccountId == acc[0].Accountid))
                return StatusCode(403, new {error = "Google Users aren't allowed to perform this action."});
            
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
}

public class ResetPasswordReq
{
    public string email { get; set; }
}

public class ResetPassword
{
    public string password { get; set; }
}