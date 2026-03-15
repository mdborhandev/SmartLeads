using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Web.Models;
using SmartLeads.Application.Users.Commands.RegisterUser;
using SmartLeads.Application.Users.Queries.LoginUser;

namespace SmartLeads.Web.Controllers;

public class AuthController : Controller
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var query = new LoginUserQuery(model.EmailOrUsername, model.Password);
            var result = await _sender.Send(query);

            // In a real MVC app, you might set a cookie here as well.
            // For now, let's assume we store the token or just redirect.
            HttpContext.Response.Cookies.Append("JwtToken", result.Token, new CookieOptions 
            { 
                HttpOnly = true, 
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new RegisterUserCommand(
                model.Username,
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName);

            var result = await _sender.Send(command);

            HttpContext.Response.Cookies.Append("JwtToken", result.Token, new CookieOptions 
            { 
                HttpOnly = true, 
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete("JwtToken");
        return RedirectToAction("Login");
    }
}
