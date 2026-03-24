using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;
using SmartLeads.Domain.Enums;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Controllers;

public class HomeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public HomeController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
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
        // Show dashboard for authenticated users
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
            var existingUser = await _unitOfWork.userRepository.GetByEmailAsync(model.AdminEmail);
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

            // Create admin user for the company as SuperAdmin
            var adminUser = new Domain.Models.User
            {
                Username = model.AdminUsername,
                Email = model.AdminEmail,
                PasswordHash = _passwordHasher.HashPassword(model.AdminPassword),
                FirstName = model.AdminFirstName,
                LastName = model.AdminLastName,
                Role = UserRole.SuperAdmin
            };

            await _unitOfWork.userRepository.AddAsync(adminUser);
            await _unitOfWork.SaveAsync();

            // Create UserCompany association
            var userCompany = new UserCompany
            {
                UserId = adminUser.Id,
                CompanyId = company.Id,
                IsDefault = true
            };
            await _unitOfWork.systemDbContext.UserCompanies.AddAsync(userCompany);

            // Create Employee record
            var employee = new Employee
            {
                CompanyId = company.Id,
                EmployeeId = $"EMP{adminUser.Id.ToString().Substring(0, 8).ToUpper()}",
                IsActive = true
            };
            await _unitOfWork.defaultDbContext.Employees.AddAsync(employee);

            // Link Employee to User
            var employeeUser = new EmployeeUser
            {
                EmployeeId = employee.Id,
                UserId = adminUser.Id,
                IsPrimary = true
            };
            await _unitOfWork.defaultDbContext.EmployeeUsers.AddAsync(employeeUser);
            await _unitOfWork.SaveAsync();

            // Generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(adminUser);

            // Set authentication cookie
            HttpContext.Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            TempData["SuccessMessage"] = $"Company '{model.CompanyName}' created successfully! Welcome aboard!";
            return RedirectToAction("Index", "Contacts");
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
