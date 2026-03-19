using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Web.Controllers;

namespace SmartLeads.Web.Controllers;

public class ColumnFilterController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public ColumnFilterController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get filtered columns for a specific list/table
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFilteredColumn(string type)
    {
        var columnFilter = await _unitOfWork.columnFilterRepository.GetColumnFilterByUserAndListNameAsync(
            UserId, CompanyId, type);

        if (columnFilter == null)
        {
            return Json(new { Success = 0 });
        }

        return Json(new { Success = 1, data = columnFilter });
    }

    /// <summary>
    /// Save or update column filter settings
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ColumnFilterCreation([FromBody] ColumnFilterRequest request, CancellationToken token)
    {
        try
        {
            var columnFilter = await _unitOfWork.columnFilterRepository.GetColumnFilterByUserAndListNameAsync(
                UserId, CompanyId, request.Type, token);

            if (columnFilter == null)
            {
                // Create new
                var newFilter = new ColumnFilter
                {
                    Id = Guid.NewGuid(),
                    CompanyId = CompanyId,
                    CreatedByUserId = UserId,
                    ListName = request.Type,
                    KeyValue = request.KeyValue,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.columnFilterRepository.AddAsync(newFilter, token);
                await _unitOfWork.SaveAsync(token);

                return Json(new { Success = 1, Message = "Data saved successfully" });
            }
            else
            {
                // Update existing
                columnFilter.KeyValue = request.KeyValue;
                columnFilter.ListName = request.Type;
                columnFilter.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.columnFilterRepository.Edit(columnFilter);
                await _unitOfWork.SaveAsync(token);

                return Json(new { Success = 1, Message = "Data updated successfully" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { Success = 0, Message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete column filter settings
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DeleteColumnFilter(string type, CancellationToken token)
    {
        try
        {
            var columnFilter = await _unitOfWork.columnFilterRepository.GetColumnFilterByUserAndListNameAsync(
                UserId, CompanyId, type, token);

            if (columnFilter == null)
            {
                return Json(new { Success = 0, Message = "No items found to delete." });
            }

            await _unitOfWork.columnFilterRepository.RemoveAsync(columnFilter);
            await _unitOfWork.SaveAsync(token);

            return Json(new { Success = 1, Message = "Deleted successfully" });
        }
        catch (Exception)
        {
            return Json(new { Success = 0, Message = "An error occurred while deleting." });
        }
    }
}
