using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Services;
using SmartLeads.Utilities.Interfaces;
using System.Text.Json;

namespace SmartLeads.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ICompanyContext _companyContext;
    private readonly SystemDbContext _systemDbContext;

    public UsersController(IUnitOfWork unitOfWork, IEmailService emailService, IConfiguration configuration, ICompanyContext companyContext, SystemDbContext systemDbContext)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
        _companyContext = companyContext;
        _systemDbContext = systemDbContext;
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

        try
        {
            // Check if user already exists with this email
            var existingUserWithEmail = await _unitOfWork.userRepository.GetByEmailAsync(model.Email);
            if (existingUserWithEmail != null)
            {
                return BadRequest(new { errors = new { Email = new[] { "Email already exists." } } });
            }

            // Check if there's already a pending invitation for this email
            var existingPendingInvite = await _unitOfWork.invitationRepository.GetPendingInvitationByEmailAndCompanyIdAsync(model.Email, companyId);
            if (existingPendingInvite != null)
            {
                return BadRequest(new { errors = new { Email = new[] { "An invitation has already been sent to this email." } } });
            }

            // Create new invitation with additional user information
            var invitation = new Invitation
            {
                Email = model.Email.ToLower().Trim(),
                Role = model.Role,
                CompanyId = companyId,
                InvitedByUserId = userId,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Status = InvitationStatus.Pending,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "FirstName", model.FirstName ?? "" },
                    { "LastName", model.LastName ?? "" },
                    { "Username", model.Username ?? "" },
                    { "EmployeeId", model.EmployeeId ?? "" },
                    { "Department", model.Department ?? "" },
                    { "Designation", model.Designation ?? "" },
                    { "PhoneNumber", model.PhoneNumber ?? "" },
                    { "Address", model.Address ?? "" },
                    { "DateOfJoining", model.DateOfJoining?.ToString("yyyy-MM-dd") ?? "" }
                })
            };

            await _unitOfWork.invitationRepository.AddAsync(invitation);
            await _unitOfWork.SaveAsync();

            // Send email with invitation link
            try
            {
                var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                var acceptLink = $"{baseUrl}/Invitations/Accept?token={invitation.Token}&email={Uri.EscapeDataString(invitation.Email)}";

                var emailBody = GetInvitationEmailTemplate(invitation.Email, model.Role.ToString(), acceptLink, invitation.ExpiresAt);

                await _emailService.SendEmailAsync(
                    invitation.Email,
                    "You're Invited to Join SmartLeads!",
                    emailBody
                );
            }
            catch (Exception emailEx)
            {
                // Log email error but don't fail the invitation
                return BadRequest(new { errors = new { __global = new[] { $"Invitation created but email failed to send: {emailEx.Message}" } } });
            }

            return Ok(new { success = true, message = "Invitation sent successfully! User will be created when they accept the invitation." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new { __global = new[] { $"Error sending invitation: {ex.Message}" } } });
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

        // Get current company ID
        var companyId = _companyContext.CurrentCompanyId;
        
        // Get role from UserCompany for this company
        var userRole = UserRole.User;
        if (companyId.HasValue)
        {
            var userCompany = await _systemDbContext.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == id && uc.CompanyId == companyId.Value);
            if (userCompany != null)
            {
                userRole = userCompany.Role;
            }
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = userRole,
            // Employee info will be loaded from Employee table based on company context
            // For now, leave these empty - they should be loaded from Employee + EmployeeUser
            EmployeeId = null,
            Department = null,
            Designation = null,
            PhoneNumber = null,
            Address = null,
            DateOfJoining = null,
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

        // Get current company ID
        var companyId = _companyContext.CurrentCompanyId;
        
        // Update user information (except password - only user can reset their own password)
        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        // Update role in UserCompany for the current company
        if (companyId.HasValue)
        {
            var userCompany = await _systemDbContext.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == id && uc.CompanyId == companyId.Value);
            
            if (userCompany != null)
            {
                userCompany.Role = model.Role;
            }
            else
            {
                // If no UserCompany exists, create one (this shouldn't normally happen)
                userCompany = new UserCompany
                {
                    UserId = id,
                    CompanyId = companyId.Value,
                    Role = model.Role,
                    IsActive = true
                };
                await _systemDbContext.UserCompanies.AddAsync(userCompany);
            }
        }

        // Note: Employee information (Department, Designation, etc.) should be updated
        // through the Employee table based on the current company context
        // This requires loading the Employee record for the current company and user

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

    private string GetInvitationEmailTemplate(string email, string role, string acceptLink, DateTime expiresAt)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: bold; }}
        .button:hover {{ background: #5a6fd6; }}
        .info-box {{ background: #e7f3ff; border-left: 4px solid #2196F3; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #888; font-size: 12px; }}
        .expiry {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 You're Invited!</h1>
            <p>Join SmartLeads Team</p>
        </div>
        <div class='content'>
            <p>Hello,</p>

            <p>You have been invited to join <strong>SmartLeads</strong> as a <strong>{role}</strong>.</p>

            <div class='info-box'>
                <strong>Invitation Details:</strong><br>
                Email: {email}<br>
                Role: {role}
            </div>

            <div class='expiry'>
                <strong>⏰ Important:</strong> This invitation will expire on <strong>{expiresAt:MMMM dd, yyyy}</strong>.
            </div>

            <p style='text-align: center;'>
                <a href='{acceptLink}' class='button'>Accept Invitation</a>
            </p>

            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #667eea; font-size: 12px;'>{acceptLink}</p>

            <p>If you have any questions, please contact the person who sent you this invitation.</p>

            <p>Best regards,<br><strong>The SmartLeads Team</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} SmartLeads. All rights reserved.</p>
            <p>This is an automated invitation, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }
}
