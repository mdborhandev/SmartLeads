using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartLeads.Web.Controllers;

public class DesignationsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DesignationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: Designations
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: Designations/Data - API endpoint
    [HttpGet]
    public async Task<IActionResult> GetDesignationsData([FromQuery] PaginationRequest request)
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

            var designations = _unitOfWork.defaultDbContext.Designations
                .Where(d => d.CompanyId == companyId && !d.IsDeleted)
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                designations = designations.Where(d =>
                    d.Name.ToLower().Contains(search) ||
                    (d.Description != null && d.Description.ToLower().Contains(search))
                );
            }

            var totalCount = designations.Count();

            // Apply sorting
            designations = request.SortField?.ToLower() switch
            {
                "name" => request.SortOrder?.ToLower() == "desc"
                    ? designations.OrderByDescending(d => d.Name)
                    : designations.OrderBy(d => d.Name),
                "isactive" => request.SortOrder?.ToLower() == "desc"
                    ? designations.OrderByDescending(d => d.IsActive)
                    : designations.OrderBy(d => d.IsActive),
                "createdat" => request.SortOrder?.ToLower() == "desc"
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

    // GET: Designations/GetAll - API endpoint for dropdown
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

            var designations = await _unitOfWork.defaultDbContext.Designations
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

    // POST: Designations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DesignationCreateViewModel model)
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
            // Check if designation name already exists for this company
            var existingDesig = await _unitOfWork.defaultDbContext.Designations
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

    // GET: Designations/Edit/5
    public async Task<IActionResult> Edit(Guid id)
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

        return View(model);
    }

    // POST: Designations/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DesignationEditViewModel model)
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

    // POST: Designations/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var designation = await _unitOfWork.designationRepository.GetByIdAsync(id);
        if (designation == null)
        {
            return NotFound();
        }

        // Soft delete
        designation.IsDeleted = true;
        designation.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "Designation deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
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
