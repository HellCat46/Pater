using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using ServiceStack;
using ServiceStack.Text;
using Web.ApplicationDbContext;
using Web.Controllers;
using Web.Models;
using Web.Models.Account;
using Web.Models.Link;
using Web.Models.View.User;

namespace Web.Areas.User.Controllers;

[Area("User")]
public class ApiController(UserDbContext context) : Controller
{
    public async Task<IActionResult> GetLinks(int pageNo)
    {
        try
        {
            if (pageNo < 1)
                return StatusCode(400, new
                {
                    error = "Page Number is too low."
                });

            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            return PartialView("~/Areas/User/Views/User/_LinkRows.cshtml", await context.Link
                .Where(link => link.AccountId == sessionAcc.id)
                .OrderByDescending(log => log.CreatedAt)
                .Skip((pageNo - 1) * 10).Take(10).ToListAsync()
            );
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
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            var linkLimit = context.Account.Where(acc => acc.id == sessionAcc.id).Select(acc => acc.linkLimit)
                .FirstOrDefault();
            if (linkLimit == null) return StatusCode(404, new { error = "Unable to Get User Information" });

            var dateTime = DateTime.Now.Subtract(TimeSpan.FromDays(DateTime.Now.Day));
            if (!link.LinkCode.IsNullOrEmpty() &&
                await context.Link.Where(l =>
                        l.isPaid == true && l.AccountId == sessionAcc.id && l.CreatedAt >= dateTime)
                    .CountAsync() >= linkLimit)
                return StatusCode(402, new
                {
                    error = "You have reached the custom Code limit that your plan allows."
                });

            if (await context.Link.AnyAsync(l => l.code == link.LinkCode))
                return StatusCode(409, new
                {
                    error = "Code Already Being used. Try another code."
                });

            await context.Link.AddAsync(new LinkModel
            {
                AccountId = sessionAcc.id,
                isPaid = !link.LinkCode.IsNullOrEmpty(),
                code = !link.LinkCode.IsNullOrEmpty() ? link.LinkCode : HomeController.GenerateRandom(8),
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now,
                name = !link.LinkName.IsNullOrEmpty()
                    ? link.LinkName
                    : DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(),
                url = link.LinkURL
            });
            await context.SaveChangesAsync();
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.CreatedLink,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", sessionAcc.id, sessionAcc.name);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while trying to Create Link." });
        }
    }

    [HttpPatch]
    public async Task<IActionResult> EditLink([FromBody] LinkView link)
    {
        try
        {
            if (link.LinkName.IsNullOrEmpty() && link.LinkURL.IsNullOrEmpty())
                return StatusCode(400, new
                {
                    error = "Required at least one of the field."
                });

            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });


            var linkRow =
                await context.Link.FirstOrDefaultAsync(l => l.code == link.LinkCode && l.AccountId == sessionAcc.id);
            if (linkRow == null)
                return StatusCode(400,
                    new { error = "Link either doesn't exist or you don't have permission to edit this link" });

            if (!link.LinkName.IsNullOrEmpty()) linkRow.name = link.LinkName;
            if (!link.LinkURL.IsNullOrEmpty()) linkRow.url = link.LinkURL;

            linkRow.LastModified = DateTime.Now;
            await context.SaveChangesAsync();
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.EditLink,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", sessionAcc.id, sessionAcc.name);

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
    public async Task<IActionResult> DeleteMultipleLink(string[] codes)
    {
        if (codes.IsNullOrEmpty())
            return StatusCode(400, new
            {
                error = "Required Parameters are missing."
            });


        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });


            var links = await context.Link.Where(link => codes.Contains(link.code)).ToListAsync();
            context.Link.RemoveRange(links);
            await context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while trying to process the request"
            });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLink(string code)
    {
        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            var linkRow =
                await context.Link.FirstOrDefaultAsync(link => link.code == code && link.AccountId == sessionAcc.id);
            if (linkRow == null)
                return StatusCode(400,
                    new { error = "Link either doesn't exist or you don't have permission to edit this link" });


            context.Link.Remove(linkRow);
            await context.SaveChangesAsync();
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.DeleteLink,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", sessionAcc.id, sessionAcc.name);

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

    [HttpGet]
    public async Task<IActionResult> LinkOtherDetails(string? detailType, string? code, long? startTimeStamp,
        long? endTimeStamp)
    {
        if (code == null || startTimeStamp == null || endTimeStamp == null || detailType == null)
            return StatusCode(400, new
            {
                error = "Required Parameters are missing."
            });

        var startTimeFrame = DateTimeOffset.FromUnixTimeSeconds(startTimeStamp.Value).LocalDateTime;
        var endTimeFrame = DateTimeOffset.FromUnixTimeSeconds(endTimeStamp.Value).LocalDateTime;
        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            var userPlan = context.Account.Where(acc => acc.id == sessionAcc.id).Select(acc => acc.plan)
                .FirstOrDefault();
            if (userPlan == null) return StatusCode(404, new { error = "Unable to Get User Information" });

            if (AccountModel.UserAnalyticsDurations(userPlan) > startTimeFrame)
                return StatusCode(403, new
                {
                    error = "Upgrade your Plan to Get this duration Information."
                });

            var link = await context.Link.FirstOrDefaultAsync(link =>
                link.AccountId == sessionAcc.id && link.code == code);
            if (link == null)
                return StatusCode(400, new
                {
                    error = "This Link is either Invalid or You don't have access to check it's details."
                });


            var baseQuery =
                context.Analytics.Where(a =>
                    a.visitedAt >= startTimeFrame && a.visitedAt <= endTimeFrame && a.LinkModelCode == code);
            IQueryable<LinkDetailsResponse> query;
            switch (detailType)
            {
                case "browser":
                    query = baseQuery.GroupBy(ana => ana.browser)
                        .Select(g => new LinkDetailsResponse { label = g.Key.ToString(), data = g.Count() });
                    break;
                case "os":
                    query = baseQuery.GroupBy(ana => ana.os)
                        .Select(g => new LinkDetailsResponse { label = g.Key.ToString(), data = g.Count() });
                    break;
                case "country":
                    query = baseQuery.GroupBy(ana => ana.country)
                        .Select(g => new LinkDetailsResponse { label = g.Key.ToString(), data = g.Count() });
                    break;
                case "city":
                    query = baseQuery.GroupBy(ana => ana.city)
                        .Select(g => new LinkDetailsResponse { label = g.Key.ToString(), data = g.Count() });
                    break;
                case "device":
                    query = baseQuery.GroupBy(ana => ana.device)
                        .Select(g => new LinkDetailsResponse { label = g.Key.ToString(), data = g.Count() });
                    break;
                default:
                    return StatusCode(404, new
                    {
                        error = "Unknown Analytics Type. Allowed Types are browser, os, country, city and device"
                    });
            }


            return Ok(await query.OrderByDescending(res => res.data).Take(10).ToListAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while trying to get Link Details"
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> LinkVisitDetails(string? code, long? startTimeStamp, long? endTimeStamp)
    {
        if (code == null || startTimeStamp == null || endTimeStamp == null)
            return StatusCode(400, new
            {
                error = "Required Parameters are missing."
            });

        var startTimeFrame = DateTimeOffset.FromUnixTimeSeconds(startTimeStamp.Value).LocalDateTime;
        var endTimeFrame = DateTimeOffset.FromUnixTimeSeconds(endTimeStamp.Value).LocalDateTime;
        Console.Write(startTimeFrame + " " + endTimeFrame);
        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            var userPlan = context.Account.Where(acc => acc.id == sessionAcc.id).Select(acc => acc.plan)
                .FirstOrDefault();
            if (userPlan == null) return StatusCode(404, new { error = "Unable to Get User Information" });

            if (AccountModel.UserAnalyticsDurations(userPlan) > startTimeFrame)
                return StatusCode(403, new
                {
                    error = "Upgrade your Plan to Get this duration Information."
                });

            var link = await context.Link.FirstOrDefaultAsync(link =>
                link.AccountId == sessionAcc.id && link.code == code);
            if (link == null)
                return StatusCode(400, new
                {
                    error = "This Link is either Invalid or You don't have access to check it's details."
                });

            if (startTimeFrame >= DateTime.Now.Subtract(TimeSpan.FromHours(24)))
                return Ok(await context.Analytics.Where(a =>
                        a.visitedAt >= startTimeFrame && a.visitedAt <= endTimeFrame && a.LinkModelCode == code)
                    .GroupBy(a => a.visitedAt.TimeOfDay).Select(g => new LinkDetailsResponse
                        { label = g.Key.ToString(), data = g.Count() })
                    .OrderBy(res => res.label).ToListAsync());


            return Ok(await context.Analytics.Where(a =>
                    a.visitedAt >= startTimeFrame && a.visitedAt <= endTimeFrame && a.LinkModelCode == code)
                .GroupBy(a => a.visitedAt.Date)
                .Select(g => new LinkDetailsResponse { label = g.Key.ToString(), data = g.Count() })
                .OrderBy(res => res.label).ToListAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new
            {
                error = "Unexpected Error while trying to get Link Details"
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> DetailsToCsv(string? linkCode, long? startTimeStamp, long? endTimeStamp)
    {
        if (linkCode == null || startTimeStamp == null || endTimeStamp == null)
            return View("ErrorPage", new ErrorView
            {
                errorCode = 400,
                errorTitle = "Bad Request",
                errorMessage = "Required Parameters are missing."
            });

        var startDate = DateTimeOffset.FromUnixTimeSeconds(startTimeStamp.Value).LocalDateTime;
        var endDate = DateTimeOffset.FromUnixTimeSeconds(endTimeStamp.Value).LocalDateTime;
        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null)
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 403,
                    errorTitle = "Server Expired",
                    errorMessage = "Session Expired. Please Login in Again."
                });


            if (!context.Link.Any(li => li.AccountId == sessionAcc.id && li.code == linkCode))
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 404,
                    errorTitle = "Not Found",
                    errorMessage = "This Link either no longer exists or you don't have access to it."
                });

            var data = await context.Analytics.Where(an =>
                an.LinkModelCode == linkCode && an.visitedAt >= startDate && endDate >= an.visitedAt).ToListAsync();
            if (data.Count == 0)
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 404,
                    errorTitle = "No Data",
                    errorMessage = "No Data to write in CSV"
                });

            var csvData = CsvSerializer.SerializeToCsv(data);
            if (csvData == null)
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 500,
                    errorTitle = "Server Error",
                    errorMessage = "Failed to Serialize Data into csv."
                });

            return new FileContentResult(csvData.ToAsciiBytes(), new MediaTypeHeaderValue("text/plain"))
            {
                FileDownloadName = "Analytics.csv"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return View("ErrorPage", new ErrorView
            {
                errorCode = 500,
                errorTitle = "Server Error",
                errorMessage = "Unexpected Error while processing the request"
            });
        }
    }

    [HttpPatch]
    public async Task<IActionResult> ChangeAvatar(IFormFile? newAvatar)
    {
        if (newAvatar == null) return StatusCode(400, new { error = "Unable to receive Image." });

        try
        {
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            var account = context.Account.FirstOrDefault(acc => acc.id == sessionAcc.id);
            if (account == null) return StatusCode(404, new { error = "Unable to Get User Information" });

            if (System.IO.File.Exists("wwwroot/UserPics/" + sessionAcc.picPath))
                System.IO.File.Delete("wwwroot/UserPics/" + sessionAcc.picPath);

            account.picPath = account.id + "." + newAvatar.FileName.Split(".").Last();
            await using (var fileStream = System.IO.File.Create("wwwroot/UserPics/" + account.picPath))
            {
                await newAvatar.CopyToAsync(fileStream);
                fileStream.Close();
            }

            context.Account.Update(account);
            await context.SaveChangesAsync();

            SessionAccountModel.SetSession(HttpContext, account);
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedAvatar,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", sessionAcc.id, sessionAcc.name);
            return Ok("/UserPics/" + account.picPath);
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
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            var account = context.Account.FirstOrDefault(acc => acc.id == sessionAcc.id);
            if (account == null) return StatusCode(404, new { error = "Unable to Get User Information" });

            if (newEmail != null)
            {
                if (context.Account.Any(acc => acc.email == newEmail))
                    return StatusCode(403, new { error = "This email address can't be linked with your account." });

                account.email = newEmail;
                if (!context.ExternalAuth.Any(ea => ea.AccountId == account.id)) // Yes, It's in the game
                    account.isVerified = false;
            }

            if (newName != null) account.name = newName;

            context.Account.Update(account);
            await context.SaveChangesAsync();
            SessionAccountModel.SetSession(HttpContext, account);

            if (!newName.IsNullOrEmpty())
                ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedName,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", sessionAcc.id, account.name);
            if (!newEmail.IsNullOrEmpty())
                ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedEmail,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", sessionAcc.id, account.name,
                    account.email);
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
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });

            var account = context.Account.FirstOrDefault(acc => acc.id == sessionAcc.id);
            if (account == null) return StatusCode(404, new { error = "Unable to Get User Information" });

            if (context.ExternalAuth.Any(ea => ea.AccountId == sessionAcc.id))
                return StatusCode(403, new { error = "Google Users aren't allowed to perform this action." });

            if (account.password != oldPassword)
                return StatusCode(400, new
                {
                    error = "Provided Old Password was incorrect!"
                });

            account.password = newPassword;
            context.Account.Update(account);
            await context.SaveChangesAsync();

            SessionAccountModel.SetSession(HttpContext, account);
            ActivityLogModel.WriteLogs(context, ActivityLogModel.Event.ChangedPassword,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown", sessionAcc.id, sessionAcc.name);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while trying to Update Avatar!" });
        }
    }
}

public class LinkDetailsResponse
    {
        public string label { get; set; }
        public int data { get; set; }
    }