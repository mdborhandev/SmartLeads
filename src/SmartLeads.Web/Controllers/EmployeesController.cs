using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Services;
using System.ComponentModel.DataAnnotations;

namespace SmartLeads.Web.Controllers;

public class EmployeesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: Employees
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: Employees/Data - API endpoint for server-side pagination and search
    [HttpGet]
    public async Task<IActionResult> GetEmployeesData([FromQuery] PaginationRequest request)
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

            // Get all employees for this company with their linked users
            var employees = _unitOfWork.defaultDbContext.Employees
                .Where(e => e.CompanyId == companyId && !e.IsDeleted)
                .Include(e => e.EmployeeUsers)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                employees = employees.Where(e =>
                    e.EmployeeId.ToLower().Contains(search) ||
                    (e.Department != null && e.Department.Name.ToLower().Contains(search)) ||
                    (e.Designation != null && e.Designation.Name.ToLower().Contains(search)) ||
                    (e.PhoneNumber != null && e.PhoneNumber.ToLower().Contains(search))
                );
            }

            var totalCount = employees.Count();

            // Apply sorting
            employees = request.SortField?.ToLower() switch
            {
                "employeeid" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.EmployeeId)
                    : employees.OrderBy(e => e.EmployeeId),
                "department" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.Department.Name)
                    : employees.OrderBy(e => e.Department.Name),
                "designation" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.Designation.Name)
                    : employees.OrderBy(e => e.Designation.Name),
                "isactive" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.IsActive)
                    : employees.OrderBy(e => e.IsActive),
                "createdat" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.CreatedAt)
                    : employees.OrderBy(e => e.CreatedAt),
                _ => employees.OrderByDescending(e => e.CreatedAt)
            };

            // Apply pagination
            var items = await employees
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeId,
                    Department = e.Department != null ? e.Department.Name : null,
                    Designation = e.Designation != null ? e.Designation.Name : null,
                    e.PhoneNumber,
                    e.IsActive,
                    e.CreatedAt,
                    Username = e.EmployeeUsers
                        .Where(eu => eu.IsPrimary)
                        .Select(eu => eu.User.Username)
                        .FirstOrDefault() ?? "N/A",
                    Email = e.EmployeeUsers
                        .Where(eu => eu.IsPrimary)
                        .Select(eu => eu.User.Email)
                        .FirstOrDefault() ?? "N/A",
                    FullName = e.EmployeeUsers
                        .Where(eu => eu.IsPrimary)
                        .Select(eu => eu.User.FirstName + " " + eu.User.LastName)
                        .FirstOrDefault() ?? "N/A"
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
            return BadRequest(new { error = "Failed to get employees", message = ex.Message });
        }
    }

    // POST: Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeCreateViewModel model)
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

        if (companyId == Guid.Empty)
        {
            return BadRequest(new { errors = new { __global = new[] { "Invalid company context." } } });
        }

        try
        {
            // Check if EmployeeId already exists for this company
            var existingEmployee = await _unitOfWork.defaultDbContext.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == model.EmployeeId && e.CompanyId == companyId);

            if (existingEmployee != null)
            {
                return BadRequest(new { errors = new { EmployeeId = new[] { "Employee ID already exists." } } });
            }

            // Create Employee record
            var employee = new Employee
            {
                CompanyId = companyId,
                EmployeeId = model.EmployeeId,
                DepartmentId = model.DepartmentId,
                DesignationId = model.DesignationId,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfJoining = model.DateOfJoining,
                IsActive = true
            };

            await _unitOfWork.defaultDbContext.Employees.AddAsync(employee);
            await _unitOfWork.SaveAsync();

            return Ok(new { success = true, message = "Employee created successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new { __global = new[] { $"Error creating employee: {ex.Message}" } } });
        }
    }

    // GET: Employees/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var employee = await _unitOfWork.defaultDbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        var model = new EmployeeEditViewModel
        {
            Id = employee.Id,
            EmployeeId = employee.EmployeeId,
            DepartmentId = employee.DepartmentId,
            DesignationId = employee.DesignationId,
            PhoneNumber = employee.PhoneNumber,
            Address = employee.Address,
            DateOfJoining = employee.DateOfJoining,
            IsActive = employee.IsActive
        };

        return View(model);
    }

    // POST: Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EmployeeEditViewModel model)
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

        var employee = await _unitOfWork.defaultDbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        // Update employee information
        employee.DepartmentId = model.DepartmentId;
        employee.DesignationId = model.DesignationId;
        employee.PhoneNumber = model.PhoneNumber;
        employee.Address = model.Address;
        employee.DateOfJoining = model.DateOfJoining;
        employee.IsActive = model.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, message = "Employee updated successfully!" });
    }

    // POST: Employees/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _unitOfWork.defaultDbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        // Soft delete
        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "Employee deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

// View Models for Employee
public class EmployeeCreateViewModel
{
    [Required]
    [Display(Name = "Employee ID")]
    public string EmployeeId { get; set; } = string.Empty;

    [Display(Name = "Department")]
    public Guid? DepartmentId { get; set; }

    [Display(Name = "Designation")]
    public Guid? DesignationId { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "Date of Joining")]
    public DateTime? DateOfJoining { get; set; }
}

public class EmployeeEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Employee ID")]
    public string EmployeeId { get; set; } = string.Empty;

    [Display(Name = "Department")]
    public Guid? DepartmentId { get; set; }

    [Display(Name = "Designation")]
    public Guid? DesignationId { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "Date of Joining")]
    public DateTime? DateOfJoining { get; set; }

    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;
}
