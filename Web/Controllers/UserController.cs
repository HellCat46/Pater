using Microsoft.AspNetCore.Mvc;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.Link;
using Web.Models.View.User;

namespace Web.Controllers;

public class UserController(UserDbContext context) : Controller
{
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
            header = new _HeaderView()
            {
                isAdmin = account.isAdmin,
                name = account.name,
                picPath = account.PicPath,
                plan = account.Plan
            },
            hasLinks = context.Link.Any(link => link.AccountId == account.id)
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
            header = new _HeaderView()
            {
                isAdmin = account.isAdmin,
                name = account.name,
                picPath = account.PicPath,
                plan = account.Plan
            },
            UserName = account.name,
            UserEmail = account.email,
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
    public IActionResult GetLinks()
    {
        try
        {
            int pageno = Convert.ToInt32(HttpContext.Request.Query["pageno"]);
            if (pageno < 1)
                return StatusCode(400, new
                {
                    error = "Page Number is too low."
                });

            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(403, new { error = "Session Expired. Please Login in Again" });
            }

            AccountModel account = AccountModel.Deserialize(bytes);

            return PartialView("_LinkRows", new _LinkRows()
            {
                links = context.Link.Where(link => link.AccountId == account.id).OrderByDescending(log => log.CreatedAt)
                    .Skip((pageno - 1) * 10).Take(10).ToList()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }

    [HttpPost]
    public IActionResult CreateLink([FromBody] LinkView link)
    {
        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(403, new
                {
                    error = "Session Expired. Please Login Again!"
                });
            }

            if (context.Link.Any(l => l.code == link.LinkCode)) 
                return StatusCode(409, new
                {
                    error = "Code Already Being used. Try another code."
                });
            
            AccountModel account = AccountModel.Deserialize(bytes);
            context.Link.Add(new LinkModel()
            {
                AccountId = account.id,
                code = (link.LinkCode != String.Empty ? link.LinkCode : GenerateRandom(8)),
                CreatedAt = DateTime.Now,
                name = (link.LinkName != String.Empty
                    ? link.LinkName
                    : DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString()),
                url = link.LinkURL
            });
            context.SaveChanges();
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.CreatedLink, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return Ok();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500, new { error = "Unexpected Error while trying to Create Link." });
        }
    }

    [HttpPatch]
    public IActionResult EditLink([FromBody] LinkView link)
    {
        try
        {
            if (link.LinkName == String.Empty && link.LinkURL == String.Empty)
                return StatusCode(400, new
                {
                    error = "At least one of the field are required to be field"
                });
            
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(403, new
                {
                    error = "Session Expired. Please Login Again!"
                });
            }

            AccountModel account = AccountModel.Deserialize(bytes);

            LinkModel? linkRow = context.Link.FirstOrDefault(l => l.code == link.LinkCode);
            if (linkRow == null) return StatusCode(404, new { error = "Link Doesn't exist" });
            
            if (linkRow.AccountId != account.id)
                return StatusCode(403, new { error = "You don't have permission to edit this link" });

            if (link.LinkName != String.Empty) linkRow.name = link.LinkName;
            if (link.LinkURL != String.Empty) linkRow.url = link.LinkURL;

            context.SaveChanges();
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while trying to Edit Link"
            });
        }
    }

    [HttpDelete]
    public IActionResult DeleteLink(string code)
    {
        try
        {
            Console.WriteLine(code);
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(403, new
                {
                    error = "Session Expired. Please Login Again!"
                });
            }

            AccountModel account = AccountModel.Deserialize(bytes);

            LinkModel? linkRow = context.Link.FirstOrDefault(link => link.code == code);
            if (linkRow == null) return StatusCode(404, new { error = "Link Doesn't exist" });
            
            if (linkRow.AccountId != account.id)
                return StatusCode(403, new { error = "You don't have permission to edit this link" });

            context.Link.Remove(linkRow);
            context.SaveChanges();
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while trying to Edit Link"
            });
        }
    }
    
    [HttpPatch]
    public IActionResult ChangeAvatar(IFormFile? newAvatar)
    {
        if (newAvatar == null)
        {
            return StatusCode(400, new { error = "Unable to receive Image." });
        }

        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(403, new
                {
                    error = "Session Expired. Please Login Again!"
                });
            }

            AccountModel account = AccountModel.Deserialize(bytes);

            if (System.IO.File.Exists("wwwroot/UserPics/" + account.PicPath))
            {
                System.IO.File.Delete("wwwroot/UserPics/" + account.PicPath);
            }

            account.PicPath = account.id + "." + newAvatar.FileName.Split(".").Last();
            using (var fileStream = System.IO.File.Create("wwwroot/UserPics/" + account.PicPath))
            {
                newAvatar.CopyTo(fileStream);
                fileStream.Close();
            }

            context.Account.Update(account);
            context.SaveChanges();
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedAvatar, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return Ok("/UserPics/" + account.PicPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while trying to Update Avatar." });
        }
    }

    [HttpPatch]
    public IActionResult UpdateNameMail(string? newName, string? newEmail)
    {
        if (newName == null && newEmail == null)
            return StatusCode(400, new
            {
                error = "At least One of the field needs to be filled."
            });

        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");
            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(403, new
                {
                    error = "Session Expired. Please Login Again!"
                });
            }

            AccountModel account = AccountModel.Deserialize(bytes);

            if (newName != null) account.name = newName;
            if (newEmail != null) account.email = newEmail;

            context.Account.Update(account);
            context.SaveChanges();
            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));

            if (newName != String.Empty)
                ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedName, account,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            if (newEmail != String.Empty)
                ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedEmail, account,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while trying to Update Info." });
        }
    }

    [HttpPatch]
    public IActionResult ChangePassword(string oldPassword, string newPassword)
    {
        try
        {
            byte[]? bytes = HttpContext.Session.Get("UserData");

            if (bytes == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(403, new
                {
                    error = "Session Expired. Please Login Again!"
                });
            }

            AccountModel account = AccountModel.Deserialize(bytes);

            if (account.password != oldPassword)
                return StatusCode(400, new
                {
                    error = "Provided Old Password was incorrect!"
                });

            account.password = newPassword;
            context.Account.Update(account);
            context.SaveChanges();

            HttpContext.Session.Set("UserData", AccountModel.Serialize(account));
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedPassword, account,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return Ok();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500, new { error = "Unexpected Error while trying to Update Avatar!" });
        }
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

        ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.LoggedOut, AccountModel.Deserialize(bytes),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

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
            context.Account.Remove(context.Account.Single(acc => acc.id == account.id));

            var links = context.Link.Where(link => link.AccountId == account.id);
            foreach (LinkModel link in links)
            {
                context.Link.Remove(link);
            }

            var logs = context.ActivityLogs.Where(logs => logs.Userid == account.id);
            foreach (ActivityLogModel log in logs)
            {
                context.ActivityLogs.Remove(log);
            }

            context.SaveChanges();
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
            code += chars[random.Next(0, chars.Length)];
        }

        return code;
    }
}