using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public UserCompanyController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
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

    // POST: UserCompany/CreateCompany - Create company and admin user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(CompanyRegistrationViewModel model)
    {
        // Require authentication
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "You must be logged in to create a company." });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = errors });
        }

        try
        {
            // Get logged-in user
            var usernameOrEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(usernameOrEmail))
            {
                return BadRequest(new { message = "Unable to identify logged-in user." });
            }

            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
            if (user == null)
            {
                return BadRequest(new { message = "Logged-in user not found." });
            }

            // Check if company name already exists
            var existingCompany = await _unitOfWork.companyRepository.GetByNameAsync(model.CompanyName);
            if (existingCompany != null)
            {
                return BadRequest(new { message = "A company with this name already exists." });
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
            await _unitOfWork.SaveAsync();

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

            // Link Employee to User
            var employeeUser = new EmployeeUser
            {
                EmployeeId = employee.Id,
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
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Company '{model.CompanyName}' created successfully! Welcome aboard!";
            return Ok(new { success = true, message = "Company created successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"An error occurred: {ex.Message}" });
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
