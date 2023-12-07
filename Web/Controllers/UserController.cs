using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
public class UserController : Controller
{
    public IActionResult Dashboard()
    {
        return View();
    }
    public IActionResult Checkout()
    {
        return View();
    }
    public IActionResult Details()
    {
        return View();
    }
    public IActionResult Settings()
    {
        return View();
        
    }
    
}