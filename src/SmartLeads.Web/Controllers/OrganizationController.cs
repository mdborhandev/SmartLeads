using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services;
using SmartLeads.Utilities.Interfaces;
using SmartLeads.Web.Filters;
using System.ComponentModel.DataAnnotations;

namespace SmartLeads.Web.Controllers;

public class OrganizationController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICompanyContext _companyContext;
    private readonly ILogger<OrganizationController> _logger;

    public OrganizationController(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICompanyContext companyContext,
        ILogger<OrganizationController> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _companyContext = companyContext;
        _logger = logger;
    }

    // ========================
    // Companies
    // ========================

    [Route("~/Companies")]
    [Route("~/Companies/Index")]
    public async Task<IActionResult> CompaniesIndex()
    {
        var companies = await _unitOfWork.companyRepository.GetCompanyDtosAsync();
        return View("~/Views/Companies/Index.cshtml", companies);
    }

    // ========================
    // Departments
    // ========================

    [Route("~/Departments")]
    [Route("~/Departments/Index")]
    public async Task<IActionResult> DepartmentsIndex()
    {
        return View("~/Views/Departments/Index.cshtml");
    }

    [Route("~/Departments/GetDepartmentsData")]
    public async Task<IActionResult> DepartmentsGetData([FromQuery] PaginationRequest request)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            if (companyId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid company context" });
            }

            var departments = _unitOfWork.dbContext.Departments
                .Where(d => d.CompanyId == companyId && !d.IsDeleted && d.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                departments = departments.Where(d =>
                    d.Name.ToLower().Contains(search) ||
                    (d.Description != null && d.Description.ToLower().Contains(search))
                );
            }

            var totalCount = departments.Count();

            departments = request.GetSortField()?.ToLower() switch
            {
                "name" => request.GetSortOrder()?.ToLower() == "desc"
                    ? departments.OrderByDescending(d => d.Name)
                    : departments.OrderBy(d => d.Name),
                "isactive" => request.GetSortOrder()?.ToLower() == "desc"
                    ? departments.OrderByDescending(d => d.IsActive)
                    : departments.OrderBy(d => d.IsActive),
                "createdat" => request.GetSortOrder()?.ToLower() == "desc"
                    ? departments.OrderByDescending(d => d.CreatedAt)
                    : departments.OrderBy(d => d.CreatedAt),
                _ => departments.OrderByDescending(d => d.CreatedAt)
            };

            var items = await departments
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    d.IsActive,
                    d.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = items,
                total = totalCount,
                page = request.Page,
                pageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to get departments", message = ex.Message });
        }
    }

    [Route("~/Departments/GetAll")]
    public async Task<IActionResult> DepartmentsGetAll()
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            var departments = await _unitOfWork.dbContext.Departments
                .Where(d => d.CompanyId == companyId && !d.IsDeleted && d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            return Ok(new { success = true, data = departments });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to get departments", message = ex.Message });
        }
    }

    [Route("~/Departments/SearchDepartment")]
    public async Task<IActionResult> DepartmentsSearch(string searchTerm = "", string selectedvalue = "")
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            var departments = await _unitOfWork.departmentRepository.SearchDepartmentsAsync(searchTerm, selectedvalue, companyId);

            return Ok(departments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = true, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Departments/Create")]
    public async Task<IActionResult> DepartmentsCreate(DepartmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { errors });
        }

        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
        var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty)
        {
            return BadRequest(new { errors = new { __global = new[] { "Invalid company context." } } });
        }

        try
        {
            var existingDept = await _unitOfWork.dbContext.Departments
                .FirstOrDefaultAsync(d => d.Name == model.Name && d.CompanyId == companyId);

            if (existingDept != null)
            {
                return BadRequest(new { errors = new { Name = new[] { "Department name already exists." } } });
            }

            var department = new Department
            {
                CompanyId = companyId,
                UserId = userId,
                Name = model.Name,
                Description = model.Description,
                IsActive = true
            };

            await _unitOfWork.departmentRepository.AddAsync(department);
            await _unitOfWork.SaveAsync();

            return Ok(new { success = true, message = "Department created successfully!", id = department.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new { __global = new[] { $"Error creating department: {ex.Message}" } } });
        }
    }

    [Route("~/Departments/Edit/{id?}")]
    public async Task<IActionResult> DepartmentsEdit(Guid id)
    {
        var department = await _unitOfWork.departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        var model = new DepartmentEditViewModel
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive
        };

        return View("~/Views/Departments/Edit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Departments/Edit/{id?}")]
    public async Task<IActionResult> DepartmentsEdit(Guid id, DepartmentEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { errors });
        }

        var department = await _unitOfWork.departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        department.Name = model.Name;
        department.Description = model.Description;
        department.IsActive = model.IsActive;
        department.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, message = "Department updated successfully!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Departments/Delete/{id}")]
    public async Task<IActionResult> DepartmentsDelete(Guid id)
    {
        var department = await _unitOfWork.departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "Department deleted successfully!";
        return RedirectToAction("DepartmentsIndex");
    }

    // ========================
    // Designations
    // ========================

    [Route("~/Designations")]
    [Route("~/Designations/Index")]
    public async Task<IActionResult> DesignationsIndex()
    {
        return View("~/Views/Designations/Index.cshtml");
    }

    [Route("~/Designations/GetDesignationsData")]
    public async Task<IActionResult> DesignationsGetData([FromQuery] PaginationRequest request)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            if (companyId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid company context" });
            }

            var designations = _unitOfWork.dbContext.Designations
                .Where(d => d.CompanyId == companyId && !d.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                designations = designations.Where(d =>
                    d.Name.ToLower().Contains(search) ||
                    (d.Description != null && d.Description.ToLower().Contains(search))
                );
            }

            var totalCount = designations.Count();

            designations = request.GetSortField()?.ToLower() switch
            {
                "name" => request.GetSortOrder()?.ToLower() == "desc"
                    ? designations.OrderByDescending(d => d.Name)
                    : designations.OrderBy(d => d.Name),
                "isactive" => request.GetSortOrder()?.ToLower() == "desc"
                    ? designations.OrderByDescending(d => d.IsActive)
                    : designations.OrderBy(d => d.IsActive),
                "createdat" => request.GetSortOrder()?.ToLower() == "desc"
                    ? designations.OrderByDescending(d => d.CreatedAt)
                    : designations.OrderBy(d => d.CreatedAt),
                _ => designations.OrderByDescending(d => d.CreatedAt)
            };

            var items = await designations
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.DepartmentId,
                    DepartmentName = d.Department != null ? d.Department.Name : null,
                    d.Description,
                    d.IsActive,
                    d.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = items,
                total = totalCount,
                page = request.Page,
                pageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to get designations", message = ex.Message });
        }
    }

    [Route("~/Designations/GetAll")]
    public async Task<IActionResult> DesignationsGetAll()
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            var designations = await _unitOfWork.dbContext.Designations
                .Where(d => d.CompanyId == companyId && !d.IsDeleted && d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            return Ok(new { success = true, data = designations });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to get designations", message = ex.Message });
        }
    }

    [Route("~/Designations/SearchDesignation")]
    public async Task<IActionResult> DesignationsSearch(string searchTerm = "", string type = "", string selectedvalue = "")
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            Guid? departmentId = null;
            if (!string.IsNullOrWhiteSpace(type) && Guid.TryParse(type, out var parsedDeptId))
            {
                departmentId = parsedDeptId;
            }

            var designations = await _unitOfWork.designationRepository.SearchDesignationsAsync(searchTerm, selectedvalue, companyId, departmentId);

            return Ok(designations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = true, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Designations/Create")]
    public async Task<IActionResult> DesignationsCreate(DesignationCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { errors });
        }

        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
        var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty)
        {
            return BadRequest(new { errors = new { __global = new[] { "Invalid company context." } } });
        }

        try
        {
            var existingDesig = await _unitOfWork.dbContext.Designations
                .FirstOrDefaultAsync(d => d.Name == model.Name && d.CompanyId == companyId);

            if (existingDesig != null)
            {
                return BadRequest(new { errors = new { Name = new[] { "Designation name already exists." } } });
            }

            var designation = new Designation
            {
                CompanyId = companyId,
                UserId = userId,
                Name = model.Name,
                DepartmentId = model.DepartmentId,
                Description = model.Description,
                IsActive = true
            };

            await _unitOfWork.designationRepository.AddAsync(designation);
            await _unitOfWork.SaveAsync();

            return Ok(new { success = true, message = "Designation created successfully!", id = designation.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new { __global = new[] { $"Error creating designation: {ex.Message}" } } });
        }
    }

    [Route("~/Designations/Edit/{id}")]
    public async Task<IActionResult> DesignationsEdit(Guid id)
    {
        var designation = await _unitOfWork.designationRepository.GetByIdAsync(id);
        if (designation == null)
        {
            return NotFound();
        }

        var model = new DesignationEditViewModel
        {
            Id = designation.Id,
            Name = designation.Name,
            DepartmentId = designation.DepartmentId,
            Description = designation.Description,
            IsActive = designation.IsActive
        };

        return View("~/Views/Designations/Edit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Designations/Edit/{id}")]
    public async Task<IActionResult> DesignationsEdit(Guid id, DesignationEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { errors });
        }

        var designation = await _unitOfWork.designationRepository.GetByIdAsync(id);
        if (designation == null)
        {
            return NotFound();
        }

        designation.Name = model.Name;
        designation.DepartmentId = model.DepartmentId;
        designation.Description = model.Description;
        designation.IsActive = model.IsActive;
        designation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, message = "Designation updated successfully!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Designations/Delete/{id}")]
    public async Task<IActionResult> DesignationsDelete(Guid id)
    {
        var designation = await _unitOfWork.designationRepository.GetByIdAsync(id);
        if (designation == null)
        {
            return NotFound();
        }

        designation.IsDeleted = true;
        designation.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "Designation deleted successfully!";
        return RedirectToAction("DesignationsIndex");
    }

    // ========================
    // UserCompany
    // ========================

    [Route("~/UserCompany/NoCompany")]
    public async Task<IActionResult> NoCompany()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View("~/Views/UserCompany/NoCompany.cshtml");
        }

        var usernameOrEmail = User.Identity?.Name;
        if (!string.IsNullOrEmpty(usernameOrEmail))
        {
            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
            if (user != null)
            {
                var companies = await _unitOfWork.userRepository.GetUserCompaniesAsync(user.Id);
                if (companies != null && companies.Any())
                {
                    return RedirectToAction("Dashboard");
                }
            }
        }

        return View("~/Views/UserCompany/NoCompany.cshtml");
    }

    [Route("~/UserCompany/Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        return View("~/Views/UserCompany/Dashboard.cshtml");
    }

    [Route("~/UserCompany/CreateCompany")]
    public async Task<IActionResult> CreateCompany()
    {
        var parentCompanies = await _unitOfWork.companyRepository.GetAllParentCompaniesAsync();
        ViewBag.ParentCompanies = parentCompanies.Select(c => new { c.Id, c.Name }).ToList();
        return View("~/Views/UserCompany/CreateCompany.cshtml");
    }

    private bool IsAjaxRequest()
    {
        return string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/UserCompany/CreateCompany")]
    public async Task<IActionResult> CreateCompany(CompanyRegistrationViewModel model)
    {
        Guid? createdCompanyId = null;
        Guid? createdEmployeeId = null;

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
            return View("~/Views/UserCompany/CreateCompany.cshtml", model);
        }

        try
        {
            var usernameOrEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(usernameOrEmail))
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { message = "Unable to identify logged-in user." });
                }

                TempData["ErrorMessage"] = "Unable to identify logged-in user.";
                return View("~/Views/UserCompany/CreateCompany.cshtml", model);
            }

            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
            if (user == null)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { message = "Logged-in user not found." });
                }

                TempData["ErrorMessage"] = "Logged-in user not found.";
                return View("~/Views/UserCompany/CreateCompany.cshtml", model);
            }

            var existingCompany = await _unitOfWork.companyRepository.GetByNameAsync(model.CompanyName);
            if (existingCompany != null)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { message = "A company with this name already exists." });
                }

                TempData["ErrorMessage"] = "A company with this name already exists.";
                return View("~/Views/UserCompany/CreateCompany.cshtml", model);
            }

            var isParent = !model.ParentCompanyId.HasValue;
            var company = new Company
            {
                Name = model.CompanyName,
                Code = model.CompanyCode,
                Email = model.CompanyEmail,
                Phone = model.CompanyPhone,
                Address = model.CompanyAddress,
                IsParent = isParent,
                ParentCompanyId = model.ParentCompanyId,
                IsActive = true
            };

            await _unitOfWork.companyRepository.AddAsync(company);

            var employee = new Employee
            {
                CompanyId = company.Id,
                EmployeeId = $"EMP{user.Id.ToString().Substring(0, 8).ToUpper()}",
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                WorkEmail = user.Email,
                DepartmentId = null,
                DesignationId = null,
                PhoneNumber = null,
                Address = "N/A",
                DateOfJoining = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.dbContext.Employees.AddAsync(employee);

            var employeeUser = new EmployeeUser
            {
                Employee = employee,
                UserId = user.Id,
                IsPrimary = true
            };
            await _unitOfWork.dbContext.EmployeeUsers.AddAsync(employeeUser);

            var existingUserCompanies = await _unitOfWork.dbContext.UserCompanies
                .Where(uc => uc.UserId == user.Id && uc.IsDefault)
                .ToListAsync();

            var shouldBeDefault = !existingUserCompanies.Any();

            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = company.Id,
                Role = UserRole.SuperAdmin,
                IsDefault = shouldBeDefault
            };
            await _unitOfWork.dbContext.UserCompanies.AddAsync(userCompany);

            _logger.LogInformation("Saving company {CompanyName} for user {UserId}", model.CompanyName, user.Id);
            await _unitOfWork.SaveAsync();
            createdCompanyId = company.Id;
            createdEmployeeId = employee.Id;

            _companyContext.ClearUserCompaniesCache(user.Id);
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
                    var employeeUsers = _unitOfWork.dbContext.EmployeeUsers
                        .Where(eu => eu.EmployeeId == createdEmployeeId.Value);
                    _unitOfWork.dbContext.EmployeeUsers.RemoveRange(employeeUsers);

                    var employee = await _unitOfWork.dbContext.Employees.FindAsync(createdEmployeeId.Value);
                    if (employee != null)
                    {
                        _unitOfWork.dbContext.Employees.Remove(employee);
                    }

                    await _unitOfWork.dbContext.SaveChangesAsync();
                }

                if (createdCompanyId.HasValue)
                {
                    var userCompanies = _unitOfWork.dbContext.UserCompanies
                        .Where(uc => uc.CompanyId == createdCompanyId.Value);
                    _unitOfWork.dbContext.UserCompanies.RemoveRange(userCompanies);

                    var company = await _unitOfWork.dbContext.Companies.FindAsync(createdCompanyId.Value);
                    if (company != null)
                    {
                        _unitOfWork.dbContext.Companies.Remove(company);
                    }

                    await _unitOfWork.dbContext.SaveChangesAsync();
                }
            }
            catch
            {
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
            return View("~/Views/UserCompany/CreateCompany.cshtml", model);
        }
    }

    [HttpPost]
    [Route("~/UserCompany/SwitchCompany")]
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

            var userCompany = await _unitOfWork.dbContext.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.CompanyId == companyId && uc.IsActive && !uc.IsDeleted);

            if (userCompany == null)
            {
                var allUserCompanies = await _unitOfWork.dbContext.UserCompanies
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

            _companyContext.SetCurrentCompany(companyId);
            HttpContext.Response.Cookies.Append("CurrentCompanyId", companyId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            var employeeUser = await _unitOfWork.dbContext.EmployeeUsers
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

    public class SwitchCompanyRequest
    {
        public Guid? CompanyId { get; set; }
    }

    [Route("~/UserCompany/GetCompanies")]
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

    [HttpPost]
    [Route("~/UserCompany/SwitchLayout")]
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

    [Route("~/UserCompany/GetLayout")]
    public IActionResult GetLayout()
    {
        var useUserCompanyLayout = HttpContext.Session.GetString("UseUserCompanyLayout") == "true";
        return Ok(new { success = true, useUserCompanyLayout });
    }

    [Route("~/UserCompany/CompanyInfo")]
    [RequireCompany]
    public async Task<IActionResult> CompanyInfo()
    {
        var currentCompanyId = _companyContext.CurrentCompanyId;
        if (!currentCompanyId.HasValue)
        {
            TempData["ErrorMessage"] = "No company selected.";
            return RedirectToAction("NoCompany");
        }

        var company = await _unitOfWork.dbContext.Companies
            .Include(c => c.ParentCompany)
            .Include(c => c.ChildCompanies.Where(cc => !cc.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == currentCompanyId.Value && !c.IsDeleted);

        if (company == null)
        {
            TempData["ErrorMessage"] = "Company not found.";
            return RedirectToAction("NoCompany");
        }

        var parentCompanies = await _unitOfWork.companyRepository.GetAllParentCompaniesAsync();
        ViewBag.ParentCompanies = parentCompanies
            .Where(c => c.Id != currentCompanyId.Value)
            .Select(c => new { c.Id, c.Name })
            .ToList();

        return View("~/Views/UserCompany/CompanyInfo.cshtml", company);
    }

    [Route("~/UserCompany/GetParentCompaniesForSelect2")]
    public async Task<IActionResult> GetParentCompaniesForSelect2(string searchTerm, string type, string selectedvalue)
    {
        try
        {
            var companies = await _unitOfWork.companyRepository.GetAllParentCompaniesAsync();

            var data = companies.Select(c => new
            {
                id = c.Id.ToString(),
                text = c.Name,
                selected = !string.IsNullOrEmpty(selectedvalue) && c.Id.ToString() == selectedvalue
            }).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                data = data.Where(x => x.text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Json(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get parent companies for Select2");
            return Json(new List<object>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/UserCompany/UpdateCompany")]
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

            if (currentUserCompany?.Role != UserRole.SuperAdmin)
            {
                return Json(new { success = false, message = "You do not have permission to update company information." });
            }

            var company = await _unitOfWork.dbContext.Companies
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
            company.ParentCompanyId = model.ParentCompanyId;
            company.IsParent = !model.ParentCompanyId.HasValue;
            company.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.dbContext.SaveChangesAsync();

            return Json(new { success = true, message = "Company information updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update company information");
            return Json(new { success = false, message = "An error occurred while updating company information." });
        }
    }
}

public class DepartmentCreateViewModel
{
    [Required]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }
}

public class DepartmentEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;
}

public class DesignationCreateViewModel
{
    [Required]
    [Display(Name = "Designation Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Department")]
    public Guid DepartmentId { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }
}

public class DesignationEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Designation Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Department")]
    public Guid DepartmentId { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;
}
