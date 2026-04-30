using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services;
using SmartLeads.Utilities.Interfaces;
using SmartLeads.Web.Filters;

namespace SmartLeads.Web.Controllers;

public class UserCompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICompanyContext _companyContext;
    private readonly ILogger<UserCompanyController> _logger;

    public UserCompanyController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, ICompanyContext companyContext, ILogger<UserCompanyController> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _companyContext = companyContext;
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
                    // User has companies - redirect to Dashboard
                    return RedirectToAction("Dashboard");
                }
            }
        }

        return View();
    }

    // GET: UserCompany/Dashboard - Default page for UserCompany Layout
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
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
                DepartmentId = null,
                DesignationId = null,
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

            // Check if user has any other companies with IsDefault = true
            var existingUserCompanies = await _unitOfWork.systemDbContext.UserCompanies
                .Where(uc => uc.UserId == user.Id && uc.IsDefault)
                .ToListAsync();

            // If user has other default companies, this new one should NOT be default
            // If this is the first company or no other defaults, set as default
            var shouldBeDefault = !existingUserCompanies.Any();

            // Create UserCompany association with SuperAdmin role
            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = company.Id,
                Role = UserRole.SuperAdmin,  // Creator is always SuperAdmin of their company
                IsDefault = shouldBeDefault
            };
            await _unitOfWork.systemDbContext.UserCompanies.AddAsync(userCompany);

            // Save all changes in one go - both contexts
            _logger.LogInformation("Saving company {CompanyName} for user {UserId}", model.CompanyName, user.Id);
            await _unitOfWork.SaveAsync();
            createdCompanyId = company.Id;
            createdEmployeeId = employee.Id;

            // Clear the user companies cache so the new company appears immediately
            _companyContext.ClearUserCompaniesCache(user.Id);
            
            // Force reload of user companies to include the new company
            await _companyContext.GetUserCompaniesAsync();

            _logger.LogInformation("Company created with ID: {CompanyId}, Employee ID: {EmployeeId}", createdCompanyId, createdEmployeeId);

            TempData["SuccessMessage"] = $"Company '{model.CompanyName}' created successfully! Welcome aboard!";
            if (IsAjaxRequest())
            {
                return Ok(new { success = true, message = "Company created successfully!" });
            }

            return RedirectToAction("Index", "Home");
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

    // POST: UserCompany/SwitchCompany - Switch to a different company
    [HttpPost]
    public async Task<IActionResult> SwitchCompany([FromBody] SwitchCompanyRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { success = false, message = "Unauthorized" });
        }

        if (request?.CompanyId == null)
        {
            return BadRequest(new { success = false, message = "Company ID is required" });
        }

        var companyId = request.CompanyId.Value;

        try
        {
            var userId = _companyContext.CurrentUserId;
            if (!userId.HasValue)
            {
                return BadRequest(new { success = false, message = "User not found" });
            }

            _logger.LogInformation("User {UserId} attempting to switch to company {CompanyId}", userId.Value, companyId);

            // Verify user belongs to this company
            var userCompany = await _unitOfWork.systemDbContext.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.CompanyId == companyId && uc.IsActive && !uc.IsDeleted);

            if (userCompany == null)
            {
                // Log detailed information for debugging
                var allUserCompanies = await _unitOfWork.systemDbContext.UserCompanies
                    .Where(uc => uc.UserId == userId.Value)
                    .ToListAsync();

                _logger.LogWarning(
                    "User {UserId} doesn't have access to company {CompanyId}. User has {Count} companies: {CompanyIds}",
                    userId.Value,
                    companyId,
                    allUserCompanies.Count,
                    string.Join(", ", allUserCompanies.Select(uc => $"{uc.CompanyId}(Active={uc.IsActive},Deleted={uc.IsDeleted})"))
                );

                return BadRequest(new { success = false, message = "You don't have access to this company" });
            }

            _logger.LogInformation("User {UserId} verified for company {CompanyId} with role {Role}",
                userId.Value, companyId, userCompany.Role);

            // Set the new company in session and cookie
            _companyContext.SetCurrentCompany(companyId);
            HttpContext.Response.Cookies.Append("CurrentCompanyId", companyId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Get and update Employee record for this user in this company
            var employeeUser = await _unitOfWork.defaultDbContext.EmployeeUsers
                .Include(eu => eu.Employee)
                .FirstOrDefaultAsync(eu => eu.UserId == userId.Value && eu.Employee.CompanyId == companyId);

            if (employeeUser != null && employeeUser.Employee != null)
            {
                HttpContext.Response.Cookies.Append("CurrentEmployeeId", employeeUser.EmployeeId.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
            }
            else
            {
                // Remove employee cookie if no employee record found
                HttpContext.Response.Cookies.Delete("CurrentEmployeeId");
            }

            _logger.LogInformation("User {UserId} switched to company {CompanyId}, EmployeeId: {EmployeeId}",
                userId, companyId, employeeUser?.EmployeeId);

            return Ok(new { success = true, message = "Company switched successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch company for user {UserId}", _companyContext.CurrentUserId);
            return BadRequest(new { success = false, message = "Failed to switch company", details = ex.Message });
        }
    }

    // DTO for SwitchCompany request
    public class SwitchCompanyRequest
    {
        public Guid? CompanyId { get; set; }
    }

    // GET: UserCompany/GetCompanies - Get user's companies (API)
    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { success = false, message = "Unauthorized" });
        }

        try
        {
            var userCompanies = await _companyContext.GetUserCompaniesAsync();
            var currentCompanyId = _companyContext.CurrentCompanyId;

            var companies = userCompanies.Select(uc => new
            {
                uc.Company.Id,
                uc.Company.Name,
                uc.Company.Code,
                Role = uc.Role.ToString(),
                IsCurrent = uc.CompanyId == currentCompanyId,
                IsDefault = uc.IsDefault
            }).ToList();

            return Ok(new { success = true, companies, currentCompanyId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get companies for user {UserId}", _companyContext.CurrentUserId);
            return BadRequest(new { success = false, message = "Failed to get companies", details = ex.Message });
        }
    }

    // POST: UserCompany/SwitchLayout - Switch between Main and UserCompany layout
    [HttpPost]
    public IActionResult SwitchLayout(bool useUserCompanyLayout)
    {
        if (useUserCompanyLayout)
        {
            HttpContext.Session.SetString("UseUserCompanyLayout", "true");
        }
        else
        {
            HttpContext.Session.Remove("UseUserCompanyLayout");
        }
        return Ok(new { success = true, useUserCompanyLayout });
    }

    // GET: UserCompany/GetLayout - Get current layout preference
    [HttpGet]
    public IActionResult GetLayout()
    {
        var useUserCompanyLayout = HttpContext.Session.GetString("UseUserCompanyLayout") == "true";
        return Ok(new { success = true, useUserCompanyLayout });
    }

    // GET: UserCompany/CompanyInfo - Show company information
    [HttpGet]
    [RequireCompany]
    public async Task<IActionResult> CompanyInfo()
    {
        var currentCompanyId = _companyContext.CurrentCompanyId;
        if (!currentCompanyId.HasValue)
        {
            TempData["ErrorMessage"] = "No company selected.";
            return RedirectToAction("NoCompany");
        }

        var company = await _unitOfWork.systemDbContext.Companies
            .FirstOrDefaultAsync(c => c.Id == currentCompanyId.Value && !c.IsDeleted);

        if (company == null)
        {
            TempData["ErrorMessage"] = "Company not found.";
            return RedirectToAction("NoCompany");
        }

        return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCompany(UpdateCompanyRequest model)
    {
        try
        {
            var currentCompanyId = _companyContext.CurrentCompanyId;
            if (!currentCompanyId.HasValue)
            {
                return Json(new { success = false, message = "No company selected." });
            }

            var userCompanies = await _companyContext.GetUserCompaniesAsync();
            var currentUserCompany = userCompanies.FirstOrDefault(uc => uc.CompanyId == currentCompanyId.Value);

            if (currentUserCompany?.Role != SmartLeads.Domain.Enums.UserRole.SuperAdmin)
            {
                return Json(new { success = false, message = "You do not have permission to update company information." });
            }

            var company = await _unitOfWork.systemDbContext.Companies
                .FirstOrDefaultAsync(c => c.Id == model.Id && !c.IsDeleted);

            if (company == null)
            {
                return Json(new { success = false, message = "Company not found." });
            }

            company.Name = model.Name;
            company.Code = model.Code;
            company.Email = model.Email;
            company.Phone = model.Phone;
            company.Address = model.Address;
            company.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.systemDbContext.SaveChangesAsync();

            return Json(new { success = true, message = "Company information updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update company information");
            return Json(new { success = false, message = "An error occurred while updating company information." });
        }
    }
}
