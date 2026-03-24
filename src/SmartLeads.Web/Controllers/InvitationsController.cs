using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;
using System.Text.Json;

namespace SmartLeads.Web.Controllers;

public class InvitationsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public InvitationsController(IUnitOfWork unitOfWork, IEmailService emailService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
    }

    // GET: Invitations
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // API: Get all invitations
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty)
        {
            return Json(new List<InvitationDto>());
        }

        var invitations = await _unitOfWork.invitationRepository.GetInvitationsDtoByCompanyIdAsync(companyId, false);
        return Json(invitations);
    }

    // GET: Invitations/Pending
    public async Task<IActionResult> Pending()
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty)
        {
            return RedirectToAction("Error", "Home");
        }

        var invitations = await _unitOfWork.invitationRepository.GetInvitationsDtoByCompanyIdAsync(companyId, true);
        return View("Index", invitations);
    }

    // GET: Invitations/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Invitations/Create (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InviteUserRequest model)
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
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty || userId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid user or company context." });
        }

        try
        {
            // Check if user already exists with this email
            var existingUser = await _unitOfWork.userRepository.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "A user with this email already exists." });
            }

            // Check if there's already a pending invitation for this email
            var existingPendingInvite = await _unitOfWork.invitationRepository.GetPendingInvitationByEmailAndCompanyIdAsync(model.Email, companyId);
            if (existingPendingInvite != null)
            {
                return BadRequest(new { message = "An invitation has already been sent to this email." });
            }

            // Create new invitation
            var invitation = new Invitation
            {
                Email = model.Email.ToLower().Trim(),
                Role = model.Role,
                CompanyId = companyId,
                InvitedByUserId = userId,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(model.ExpiryDays),
                Status = InvitationStatus.Pending,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, string>())
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
                return BadRequest(new { message = $"Invitation created but email failed to send: {emailEx.Message}" });
            }

            return Ok(new { message = "Invitation sent successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error sending invitation: {ex.Message}" });
        }
    }

    // GET: Invitations/Accept
    [AllowAnonymous]
    public async Task<IActionResult> Accept(string token, string email)
    {
        try
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid invitation link.";
                return RedirectToAction("Login", "Auth");
            }

            var model = new AcceptInvitationRequest
            {
                Token = token,
                Email = email
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading invitation: {ex.Message}";
            return RedirectToAction("Login", "Auth");
        }
    }

    // POST: Invitations/Reject
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string token, string email, string reason)
    {
        try
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Invalid invitation link." });
            }

            // Find the invitation
            var invitation = await _unitOfWork.invitationRepository.GetByEmailAndTokenAsync(email, token);
            
            if (invitation == null)
            {
                return BadRequest(new { message = "Invalid invitation." });
            }

            // Check if already processed
            if (invitation.Status != InvitationStatus.Pending)
            {
                return BadRequest(new { message = "This invitation has already been processed." });
            }

            // Update invitation status
            invitation.Status = InvitationStatus.Rejected;
            invitation.RejectedReason = reason;
            await _unitOfWork.invitationRepository.EditAsync(invitation);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Invitation rejected successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error rejecting invitation: {ex.Message}" });
        }
    }

    // POST: Invitations/Accept
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(AcceptInvitationRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Find invitation
            var invitation = await _unitOfWork.invitationRepository.GetByEmailAndTokenAsync(model.Email, model.Token);

            if (invitation == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid invitation token or email.");
                return View(model);
            }

            // Check if invitation has a valid company
            if (invitation.CompanyId == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "This invitation is invalid (no company associated).");
                return View(model);
            }

            // Check if invitation is expired
            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _unitOfWork.invitationRepository.EditAsync(invitation);
                await _unitOfWork.SaveAsync();
                ModelState.AddModelError(string.Empty, "This invitation has expired.");
                return View(model);
            }

            // Check if already accepted
            if (invitation.IsAccepted || invitation.Status == InvitationStatus.Accepted)
            {
                ModelState.AddModelError(string.Empty, "This invitation has already been accepted.");
                return View(model);
            }

            // Check if invitation is cancelled or rejected
            if (invitation.Status == InvitationStatus.Cancelled || invitation.Status == InvitationStatus.Rejected)
            {
                ModelState.AddModelError(string.Empty, "This invitation is no longer valid.");
                return View(model);
            }

            // Check if username is already taken in this company
            var existingUser = await _unitOfWork.userRepository.GetByUsernameAndCompanyIdAsync(model.Username, invitation.CompanyId);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "This username is already taken in this company.");
                return View(model);
            }

            // Create user account with the role from invitation and username from request
            var passwordHasher = HttpContext.RequestServices.GetRequiredService<IPasswordHasher>();
            var jwtTokenGenerator = HttpContext.RequestServices.GetRequiredService<IJwtTokenGenerator>();

            var user = new Domain.Models.User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHasher.HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = invitation.Role
            };

            await _unitOfWork.userRepository.AddAsync(user);

            // Create UserCompany association
            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = invitation.CompanyId,
                IsDefault = true
            };
            await _unitOfWork.systemDbContext.UserCompanies.AddAsync(userCompany);

            // Create Employee record
            var employee = new Employee
            {
                CompanyId = invitation.CompanyId,
                EmployeeId = $"EMP{user.Id.ToString().Substring(0, 8).ToUpper()}",
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
            await _unitOfWork.SaveAsync();

            // Update invitation status
            invitation.IsAccepted = true;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.Status = InvitationStatus.Accepted;
            await _unitOfWork.invitationRepository.EditAsync(invitation);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Your account has been created successfully! You can now login.";
            return RedirectToAction("Login", "Auth");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error accepting invitation: {ex.Message}");
            return View(model);
        }
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
