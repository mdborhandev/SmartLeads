using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Web.Models;
using SmartLeads.Application.Users.Commands.RegisterUser;
using SmartLeads.Application.Users.Queries.LoginUser;
using SmartLeads.Application.Users.Commands.ForgotPassword;
using SmartLeads.Application.Users.Commands.ResetPassword;

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

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new ForgotPasswordCommand(model.Email);
            await _sender.Send(command);

            TempData["SuccessMessage"] = "If an account exists with that email, we've sent a password reset link.";
            return RedirectToAction("ForgotPasswordConfirmation");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new ResetPasswordCommand(model.Email, model.Token, model.NewPassword);
            await _sender.Send(command);

            TempData["SuccessMessage"] = "Your password has been reset successfully. You can now log in.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
