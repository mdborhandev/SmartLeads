using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;
using SmartLeads.Utilities.Interfaces;
using SmartLeads.Infrastructure.Services.Interface;
using SmartLeads.Infrastructure.Services.Implementation;

namespace SmartLeads.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvitationService _invitationService;

    public UsersController(IUnitOfWork unitOfWork, IInvitationService invitationService)
    {
        _unitOfWork = unitOfWork;
        _invitationService = invitationService;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: Users/Data - API endpoint for server-side pagination and search
    [HttpGet]
    public async Task<IActionResult> GetUsersData([FromQuery] PaginationRequest request)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
        
        if (companyId == Guid.Empty)
        {
            return BadRequest(new { error = "Invalid company context" });
        }

        var result = await _unitOfWork.userRepository.GetUsersPagedAsync(companyId, request);
        return Ok(result);
    }

    // POST: Users/Create - Send Invitation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
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

        // Check if username exists
        var existingUser = await _unitOfWork.userRepository.GetByUsernameAsync(model.Username);
        if (existingUser != null)
        {
            return BadRequest(new { errors = new { Username = new[] { "Username already exists." } } });
        }

        // Check if email exists
        var existingEmail = await _unitOfWork.userRepository.GetByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            return BadRequest(new { errors = new { Email = new[] { "Email already exists." } } });
        }

        // Get current user's company ID
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty || userId == Guid.Empty)
        {
            return BadRequest(new { errors = new { __global = new[] { "Invalid user or company context." } } });
        }

        // Send invitation instead of creating user directly
        var result = await _invitationService.InviteUserAsync(
            model.Email,
            model.Role,
            7, // 7 days expiry
            companyId,
            userId,
            model.FirstName,
            model.LastName,
            model.Username,
            model.EmployeeId,
            model.Department,
            model.Designation,
            model.PhoneNumber,
            model.Address,
            model.DateOfJoining
        );

        if (result.Success)
        {
            return Ok(new { success = true, message = "Invitation sent successfully! User will be created when they accept the invitation." });
        }
        else
        {
            return BadRequest(new { errors = new { __global = new[] { result.Message } } });
        }
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _unitOfWork.userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            EmployeeId = user.EmployeeId,
            Department = user.Department,
            Designation = user.Designation,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            DateOfJoining = user.DateOfJoining,
            IsActive = user.IsActive
        };

        return View(model);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditUserViewModel model)
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

        var user = await _unitOfWork.userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Update user information (except password - only user can reset their own password)
        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Role = model.Role;
        user.EmployeeId = model.EmployeeId;
        user.Department = model.Department;
        user.Designation = model.Designation;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.DateOfJoining = model.DateOfJoining;
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, message = "User updated successfully!" });
    }

    // POST: Users/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _unitOfWork.userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        await _unitOfWork.userRepository.RemoveAsync(id);
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "User deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
