using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using System.ComponentModel.DataAnnotations;

namespace SmartLeads.Web.Controllers;

public class VariablesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public VariablesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SearchType(string searchTerm = "", string selectedvalue = "")
    {
        try
        {
            var types = _unitOfWork.variableRepository.GetAllCommonDataType(searchTerm, selectedvalue);
            return Ok(types);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = true, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchVariable(string searchTerm = "", string type = "", string selectedvalue = "")
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { error = "CompanyId claim not found" });
            }

            var companyId = Guid.Parse(companyIdClaim);

            var query = _unitOfWork.defaultDbContext.Variables
                .Where(v => v.CompanyId == companyId && !v.IsDeleted && v.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(v => v.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(v => v.Value.ToLower().Contains(searchLower));
            }

            var variables = await query
                .OrderBy(v => v.SortOrder)
                .ThenBy(v => v.Value)
                .Take(20)
                .Select(v => new SelectOptionDto
                {
                    id = v.Id.ToString(),
                    text = v.Value,
                    selected = selectedvalue != "" && v.Id.ToString() == selectedvalue
                })
                .ToListAsync();

            return Ok(variables);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = true, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetVariablesData([FromQuery] PaginationRequest request)
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

            var variables = _unitOfWork.defaultDbContext.Variables
                .Where(v => v.CompanyId == companyId && !v.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                variables = variables.Where(v =>
                    v.Type.ToLower().Contains(search) ||
                    v.Value.ToLower().Contains(search) ||
                    (v.Description != null && v.Description.ToLower().Contains(search))
                );
            }

            var totalCount = variables.Count();

            variables = request.SortField?.ToLower() switch
            {
                "type" => request.SortOrder?.ToLower() == "desc"
                    ? variables.OrderByDescending(v => v.Type)
                    : variables.OrderBy(v => v.Type),
                "value" => request.SortOrder?.ToLower() == "desc"
                    ? variables.OrderByDescending(v => v.Value)
                    : variables.OrderBy(v => v.Value),
                "isactive" => request.SortOrder?.ToLower() == "desc"
                    ? variables.OrderByDescending(v => v.IsActive)
                    : variables.OrderBy(v => v.IsActive),
                "createdat" => request.SortOrder?.ToLower() == "desc"
                    ? variables.OrderByDescending(v => v.CreatedAt)
                    : variables.OrderBy(v => v.CreatedAt),
                _ => variables.OrderByDescending(v => v.CreatedAt)
            };

            var items = await variables
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(v => new
                {
                    v.Id,
                    v.Type,
                    v.Value,
                    v.Value1,
                    v.Value2,
                    v.Value3,
                    v.Description,
                    v.SortOrder,
                    v.IsActive,
                    v.CreatedAt
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
            return BadRequest(new { error = "Failed to get variables", message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTypes()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim))
        {
            return BadRequest(new { success = false, message = "Invalid company context." });
        }

        var companyId = Guid.Parse(companyIdClaim);
        var types = await _unitOfWork.variableRepository.GetTypesAsync(companyId);

        return Ok(new { success = true, data = types });
    }

    [HttpGet]
    public async Task<IActionResult> GetByType(string type)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim))
        {
            return BadRequest(new { success = false, message = "Invalid company context." });
        }

        var companyId = Guid.Parse(companyIdClaim);
        var variables = await _unitOfWork.variableRepository.GetByTypeAsync(type, companyId);

        var result = variables.Select(v => new VariableDto
        {
            Id = v.Id,
            Type = v.Type,
            Value = v.Value,
            Value1 = v.Value1,
            Value2 = v.Value2,
            Value3 = v.Value3,
            Description = v.Description,
            SortOrder = v.SortOrder,
            IsActive = v.IsActive,
            CreatedAt = v.CreatedAt
        }).ToList();

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] VariableDto model)
    {
        var errors = new Dictionary<string, List<string>>();
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(companyIdClaim))
        {
            return BadRequest(new { success = false, message = "Invalid company context. Please login again." });
        }

        var companyId = Guid.Parse(companyIdClaim);

        if (string.IsNullOrWhiteSpace(model.Type))
        {
            errors["Type"] = new List<string> { "Type is required" };
        }

        if (string.IsNullOrWhiteSpace(model.Value))
        {
            errors["Value"] = new List<string> { "Value is required" };
        }

        if (errors.Any())
        {
            return BadRequest(new { success = false, errors, message = "Validation failed. Please check the form." });
        }

        var isEdit = model.Id.HasValue && model.Id.Value != Guid.Empty;

        try
        {
            var variableRepo = _unitOfWork.variableRepository;

            if (isEdit)
            {
                var variable = await variableRepo.GetByIdAsync(model.Id!.Value);
                if (variable == null)
                {
                    return NotFound(new { success = false, message = "Variable not found." });
                }

                // Check for duplicate
                var duplicate = await variableRepo.GetByTypeAndValueAsync(model.Type!, model.Value!, companyId);
                if (duplicate != null && duplicate.Id != variable.Id)
                {
                    return BadRequest(new { success = false, errors = new { Value = new List<string> { "Value already exists for this type." } }, message = "Value already exists." });
                }

                variable.Type = model.Type;
                variable.Value = model.Value;
                variable.Value1 = model.Value1;
                variable.Value2 = model.Value2;
                variable.Value3 = model.Value3;
                variable.Description = model.Description;
                variable.SortOrder = model.SortOrder;
                variable.IsActive = model.IsActive;
                variable.UpdatedAt = DateTime.UtcNow;

                variableRepo.Edit(variable);
                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, message = "Variable updated successfully!" });
            }
            else
            {
                // Check for duplicate
                var existing = await variableRepo.GetByTypeAndValueAsync(model.Type!, model.Value!, companyId);
                if (existing != null)
                {
                    return BadRequest(new { success = false, errors = new { Value = new List<string> { "Value already exists for this type." } }, message = "Value already exists." });
                }

                var variable = new Domain.Models.Variable
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    Type = model.Type,
                    Value = model.Value,
                    Value1 = model.Value1,
                    Value2 = model.Value2,
                    Value3 = model.Value3,
                    Description = model.Description,
                    SortOrder = model.SortOrder,
                    IsActive = model.IsActive
                };

                await variableRepo.AddAsync(variable);
                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, message = "Variable created successfully!" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "An error occurred while saving variable.", details = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var variable = await _unitOfWork.variableRepository.GetByIdAsync(id);
        if (variable == null)
        {
            return NotFound(new { success = false, message = "Variable not found." });
        }

        await _unitOfWork.variableRepository.SoftDeleteAsync(id);
        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, message = "Variable deleted successfully!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteByType(string type)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim))
        {
            return BadRequest(new { success = false, message = "Invalid company context." });
        }

        var companyId = Guid.Parse(companyIdClaim);
        var variables = await _unitOfWork.variableRepository.GetByTypeAsync(type, companyId);

        foreach (var variable in variables)
        {
            await _unitOfWork.variableRepository.SoftDeleteAsync(variable.Id);
        }

        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, message = $"All variables of type '{type}' deleted successfully!" });
    }
}