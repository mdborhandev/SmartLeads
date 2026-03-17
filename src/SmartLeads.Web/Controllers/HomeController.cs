using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Services.Interface;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Web.Controllers;

public class HomeController : Controller
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(IUserService userService, IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Landing()
    {
        // If user is already logged in, redirect to contacts
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Contacts");
        }

        return View();
    }

    public IActionResult Index()
    {
        // If user is already logged in, redirect to contacts
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Contacts");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Components()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CreateCompany()
    {
        // If user is already logged in, redirect to home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Contacts");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(CompanyRegistrationViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Contacts");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Check if company name already exists
            var existingCompany = await _unitOfWork.companyRepository.GetByNameAsync(model.CompanyName);
            if (existingCompany != null)
            {
                ModelState.AddModelError(string.Empty, "A company with this name already exists.");
                return View(model);
            }

            // Check if admin email already exists
            var existingUser = await _userService.GetUserByUsernameOrEmailAsync(model.AdminEmail);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return View(model);
            }

            // Create company first
            var company = new Company
            {
                Name = model.CompanyName,
                Code = model.CompanyCode,
                Email = model.CompanyEmail,
                Phone = model.CompanyPhone,
                Address = model.CompanyAddress,
                IsParent = true,
                IsActive = true
            };

            await _unitOfWork.companyRepository.AddAsync(company);
            await _unitOfWork.SaveAsync();

            // Register admin user for the company
            var registerResult = await _userService.RegisterAsync(
                model.AdminUsername,
                model.AdminEmail,
                model.AdminPassword,
                model.AdminFirstName,
                model.AdminLastName,
                company.Id // Associate user with company
            );

            if (registerResult.Success)
            {
                // Set authentication cookie
                HttpContext.Response.Cookies.Append("JwtToken", registerResult.Token!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                TempData["SuccessMessage"] = $"Company '{model.CompanyName}' created successfully! Welcome aboard!";
                return RedirectToAction("Index", "Contacts");
            }
            else
            {
                // Rollback company creation if user registration fails
                _unitOfWork.companyRepository.Remove(company.Id);
                await _unitOfWork.SaveAsync();

                ModelState.AddModelError(string.Empty, registerResult.Error);
                return View(model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return View(model);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
