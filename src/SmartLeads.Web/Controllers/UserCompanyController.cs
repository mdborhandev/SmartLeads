using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Controllers;

public class UserCompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<UserCompanyController> _logger;

    public UserCompanyController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, ILogger<UserCompanyController> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    // GET: UserCompany/NoCompany - Displayed when user is not associated with any company
    [HttpGet]
    public async Task<IActionResult> NoCompany()
    {
        // Check if user is logged in
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // User is not logged in, show the page anyway so they can create a company
            return View();
        }

        // Get user's companies if any
        var usernameOrEmail = User.Identity?.Name;
        if (!string.IsNullOrEmpty(usernameOrEmail))
        {
            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
            if (user != null)
            {
                var companies = await _unitOfWork.userRepository.GetUserCompaniesAsync(user.Id);
                if (companies != null && companies.Any())
                {
                    // User has companies, redirect to contacts
                    return RedirectToAction("Index", "Contacts");
                }
            }
        }

        return View();
    }

    // GET: UserCompany/CreateCompany - Show create company form
    [HttpGet]
    public IActionResult CreateCompany()
    {
        return View();
    }

    private bool IsAjaxRequest()
    {
        return string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    // POST: UserCompany/CreateCompany - Create company and admin user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(CompanyRegistrationViewModel model)
    {
        Guid? createdCompanyId = null;
        Guid? createdEmployeeId = null;

        // Require authentication
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            if (IsAjaxRequest())
            {
                return Unauthorized(new { message = "You must be logged in to create a company." });
            }

            TempData["ErrorMessage"] = "You must be logged in to create a company.";
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            if (IsAjaxRequest())
            {
                return BadRequest(new { errors = errors });
            }

            TempData["ErrorMessage"] = string.Join(" ", errors);
            return View(model);
        }

        try
        {
            // Get logged-in user
            var usernameOrEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(usernameOrEmail))
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { message = "Unable to identify logged-in user." });
                }

                TempData["ErrorMessage"] = "Unable to identify logged-in user.";
                return View(model);
            }

            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
            if (user == null)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { message = "Logged-in user not found." });
                }

                TempData["ErrorMessage"] = "Logged-in user not found.";
                return View(model);
            }

            // Check if company name already exists
            var existingCompany = await _unitOfWork.companyRepository.GetByNameAsync(model.CompanyName);
            if (existingCompany != null)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { message = "A company with this name already exists." });
                }

                TempData["ErrorMessage"] = "A company with this name already exists.";
                return View(model);
            }

            // Create company
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

            // Create Employee record for the logged-in user with default values
            var employee = new Employee
            {
                CompanyId = company.Id,
                EmployeeId = $"EMP{user.Id.ToString().Substring(0, 8).ToUpper()}",
                Department = "N/A",
                Designation = "N/A",
                PhoneNumber = null,
                Address = "N/A",
                DateOfJoining = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.defaultDbContext.Employees.AddAsync(employee);

            // Keep the user mapping in the same default-db save as the employee record.
            var employeeUser = new EmployeeUser
            {
                Employee = employee,
                UserId = user.Id,
                IsPrimary = true
            };
            await _unitOfWork.defaultDbContext.EmployeeUsers.AddAsync(employeeUser);

            // Create UserCompany association
            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = company.Id,
                IsDefault = true
            };
            await _unitOfWork.systemDbContext.UserCompanies.AddAsync(userCompany);

            // Save all changes in one go - both contexts
            _logger.LogInformation("Saving company {CompanyName} for user {UserId}", model.CompanyName, user.Id);
            await _unitOfWork.SaveAsync();
            createdCompanyId = company.Id;
            createdEmployeeId = employee.Id;
            _logger.LogInformation("Company created with ID: {CompanyId}, Employee ID: {EmployeeId}", createdCompanyId, createdEmployeeId);

            TempData["SuccessMessage"] = $"Company '{model.CompanyName}' created successfully! Welcome aboard!";
            if (IsAjaxRequest())
            {
                return Ok(new { success = true, message = "Company created successfully!" });
            }

            return RedirectToAction("Index", "Contacts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create company {CompanyName}. Error: {Error}", model.CompanyName, ex.Message);
            try
            {
                if (createdEmployeeId.HasValue)
                {
                    var employeeUsers = _unitOfWork.defaultDbContext.EmployeeUsers
                        .Where(eu => eu.EmployeeId == createdEmployeeId.Value);
                    _unitOfWork.defaultDbContext.EmployeeUsers.RemoveRange(employeeUsers);

                    var employee = await _unitOfWork.defaultDbContext.Employees.FindAsync(createdEmployeeId.Value);
                    if (employee != null)
                    {
                        _unitOfWork.defaultDbContext.Employees.Remove(employee);
                    }

                    await _unitOfWork.defaultDbContext.SaveChangesAsync();
                }

                if (createdCompanyId.HasValue)
                {
                    var userCompanies = _unitOfWork.systemDbContext.UserCompanies
                        .Where(uc => uc.CompanyId == createdCompanyId.Value);
                    _unitOfWork.systemDbContext.UserCompanies.RemoveRange(userCompanies);

                    var company = await _unitOfWork.systemDbContext.Companies.FindAsync(createdCompanyId.Value);
                    if (company != null)
                    {
                        _unitOfWork.systemDbContext.Companies.Remove(company);
                    }

                    await _unitOfWork.systemDbContext.SaveChangesAsync();
                }
            }
            catch
            {
                // Preserve the original failure response; best-effort cleanup only.
            }

            if (IsAjaxRequest())
            {
                return BadRequest(new
                {
                    message = "Failed to create company. Please try again.",
                    details = ex.Message
                });
            }

            TempData["ErrorMessage"] = $"Failed to create company. {ex.Message}";
            return View(model);
        }
    }

    // POST: UserCompany/JoinCompany - Join existing company with code
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinCompany(string companyCode)
    {
        if (string.IsNullOrEmpty(companyCode))
        {
            TempData["ErrorMessage"] = "Please enter a valid company code";
            return RedirectToAction("NoCompany");
        }

        // TODO: Implement company joining logic
        // For now, just redirect
        return RedirectToAction("Index", "Contacts");
    }
}
