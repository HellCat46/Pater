using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using ServiceStack;
using ServiceStack.Text;
using Web.ApplicationDbContext;
using Web.Models;
using Web.Models.Account;
using Web.Models.View.Admin;

namespace Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ApiController(UserDbContext context) : Controller
{
    public async Task<IActionResult> GetUserLogs(int pageNo, int userId)
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


            if (sessionAcc.isAdmin != true)
                return StatusCode(403, new { error = "This action requires Admin Access" });

            return PartialView("~/Areas/Admin/Views/Admin/_UserLogRows.cshtml", await context.ActivityLogs.Where(log => log.Userid == userId)
                .Select(log => new UserLog { date = log.date, action = log.Action })
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
            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });
            if (!sessionAcc.isAdmin) return StatusCode(403, new { error = "This action requires Admin Access." });

            var userAccount = await context.Account.Include(acc => acc.ExternalAuth)
                .FirstOrDefaultAsync(acc => acc.id == data.UserId);
            if (userAccount == null)
                return StatusCode(404, new { error = "User no longer exists" });
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

            var sessionAcc = SessionAccountModel.GetSession(HttpContext);
            if (sessionAcc == null) return StatusCode(403, new { error = "Session Expired. Please Login in Again" });
            if (sessionAcc.isAdmin != true)
                return StatusCode(403, new { error = "This action requires Admin Access" });

            return PartialView("~/Areas/Admin/Views/Admin/_LogRows.cshtml",
                await context.ActivityLogs.OrderByDescending(log => log.date).Skip((pageNo - 1) * 10).Take(10)
                    .ToListAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Unexpected Error while processing the request" });
        }
    }

    public IActionResult GetLogsAsCsv(DateTime? startDate, DateTime? endDate)
    {
        if (startDate == null || endDate == null)
            return StatusCode(400, new
            {
                error = "Required Parameters are missing"
            });
        Console.Write(startDate + " " + endDate);
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

            if (sessionAcc.isAdmin != true) return RedirectToAction("Dashboard", "User");

            var logs = context.ActivityLogs.Where(al => al.date >= startDate && al.date <= endDate).ToList();
            if (logs.Count == 0)
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 404,
                    errorTitle = "No Logs",
                    errorMessage = "No logs to Download in the given Time Duration."
                });

            var csv = CsvSerializer.SerializeToCsv(logs);
            if (csv == null)
                return View("ErrorPage", new ErrorView
                {
                    errorCode = 500,
                    errorTitle = "Server Error",
                    errorMessage = "Failed to Serialize Logs into csv."
                });


            return new FileContentResult(csv.ToAsciiBytes(), new MediaTypeHeaderValue("text/plain"))
            {
                FileDownloadName = "Logs.csv"
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
}