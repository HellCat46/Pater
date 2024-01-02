using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ApplicationDbContext;
using Web.Models.Account;
using Web.Models.Link;
using Web.Models.View.User;

namespace Web.Controllers;

public class UserController(UserDbContext context) : Controller
{
    public async Task<IActionResult> Dashboard()
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
            hasLinks = await context.Link.AnyAsync(link => link.AccountId == account.id)
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
    public async Task<IActionResult> GetLinks()
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
                links = await context.Link.Where(link => link.AccountId == account.id).OrderByDescending(log => log.CreatedAt)
                    .Skip((pageno - 1) * 10).Take(10).ToListAsync()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateLink([FromBody] LinkView link)
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

            if (await context.Link.AnyAsync(l => l.code == link.LinkCode)) 
                return StatusCode(409, new
                {
                    error = "Code Already Being used. Try another code."
                });
            
            AccountModel account = AccountModel.Deserialize(bytes);
            await context.Link.AddAsync(new LinkModel()
            {
                AccountId = account.id,
                code = (link.LinkCode != String.Empty ? link.LinkCode : GenerateRandom(8)),
                CreatedAt = DateTime.Now,
                name = (link.LinkName != String.Empty
                    ? link.LinkName
                    : DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString()),
                url = link.LinkURL
            });
            await context.SaveChangesAsync();
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
    public async Task<IActionResult> EditLink([FromBody] LinkView link)
    {
        try
        {
            if (link.LinkName == String.Empty && link.LinkURL == String.Empty)
                return StatusCode(400, new
                {
                    error = "Required at least one of the field."
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

            LinkModel? linkRow = await context.Link.FirstOrDefaultAsync(l => l.code == link.LinkCode);
            if (linkRow == null) return StatusCode(404, new { error = "Link Doesn't exist" });
            
            if (linkRow.AccountId != account.id)
                return StatusCode(403, new { error = "You don't have permission to edit this link" });

            if (link.LinkName != String.Empty) linkRow.name = link.LinkName;
            if (link.LinkURL != String.Empty) linkRow.url = link.LinkURL;

            await context.SaveChangesAsync();
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
    public async Task<IActionResult> DeleteLink(string code)
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

            LinkModel? linkRow = await context.Link.FirstOrDefaultAsync(link => link.code == code);
            if (linkRow == null) return StatusCode(404, new { error = "Link Doesn't exist" });
            
            if (linkRow.AccountId != account.id)
                return StatusCode(403, new { error = "You don't have permission to edit this link" });

            context.Link.Remove(linkRow);
            await context.SaveChangesAsync();
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
    public async Task<IActionResult> ChangeAvatar(IFormFile? newAvatar)
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
            await using (var fileStream = System.IO.File.Create("wwwroot/UserPics/" + account.PicPath))
            {
                await newAvatar.CopyToAsync(fileStream);
                fileStream.Close();
            }

            context.Account.Update(account);
            await context.SaveChangesAsync();
            
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
    public async Task<IActionResult> UpdateNameMail(string? newName, string? newEmail)
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
            await context.SaveChangesAsync();
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
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
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
            await context.SaveChangesAsync();

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

    public async Task<IActionResult> DeleteAccount()
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
            AccountModel? userAcc = await context.Account.SingleOrDefaultAsync(acc => acc.id == account.id);
            if (userAcc == null) return RedirectToAction("Index", "Home");
                
            context.Account.Remove(userAcc);

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

            await context.SaveChangesAsync();
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