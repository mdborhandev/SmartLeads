using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartLeads.Web.Controllers;

public class DepartmentsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: Departments
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: Departments/Data - API endpoint
    [HttpGet]
    public async Task<IActionResult> GetDepartmentsData([FromQuery] PaginationRequest request)
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

            var departments = _unitOfWork.defaultDbContext.Departments
                .Where(d => d.CompanyId == companyId && !d.IsDeleted && d.IsActive)
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                departments = departments.Where(d =>
                    d.Name.ToLower().Contains(search) ||
                    (d.Description != null && d.Description.ToLower().Contains(search))
                );
            }

            var totalCount = departments.Count();

            // Apply sorting
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

    // GET: Departments/GetAll - API endpoint for dropdown
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            var departments = await _unitOfWork.defaultDbContext.Departments
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

    // GET: Departments/SearchDepartment - Select2 search endpoint
    [HttpGet]
    public async Task<IActionResult> SearchDepartment(string searchTerm = "", string selectedvalue = "")
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

    // POST: Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentCreateViewModel model)
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
            // Check if department name already exists for this company
            var existingDept = await _unitOfWork.defaultDbContext.Departments
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

    // GET: Departments/Edit/5
    public async Task<IActionResult> Edit(Guid id)
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

        return View(model);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DepartmentEditViewModel model)
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

    // POST: Departments/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var department = await _unitOfWork.departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        // Soft delete
        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "Department deleted successfully!";
        return RedirectToAction(nameof(Index));
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
