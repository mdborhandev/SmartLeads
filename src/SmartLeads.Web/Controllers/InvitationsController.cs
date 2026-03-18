using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services.Implementation;

namespace SmartLeads.Web.Controllers;

public class InvitationsController : Controller
{
    private readonly IInvitationService _invitationService;
    private readonly IUnitOfWork _unitOfWork;

    public InvitationsController(IInvitationService invitationService, IUnitOfWork unitOfWork)
    {
        _invitationService = invitationService;
        _unitOfWork = unitOfWork;
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

        var invitations = await _invitationService.GetAllInvitationsByCompanyIdAsync(companyId);
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

        var invitations = await _invitationService.GetPendingInvitationsByCompanyIdAsync(companyId);
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

        var result = await _invitationService.InviteUserAsync(
            model.Email,
            model.Role,
            model.ExpiryDays,
            companyId,
            userId
        );

        if (result.Success)
        {
            return Ok(new { message = result.Message });
        }
        else
        {
            return BadRequest(new { message = result.Message });
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

        var result = await _invitationService.AcceptInvitationAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Your account has been created successfully! You can now login.";
            return RedirectToAction("Login", "Auth");
        }
        else
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }
    }
}
