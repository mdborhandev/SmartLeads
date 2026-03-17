using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Services.Implementation;

namespace SmartLeads.Web.Controllers;

[Authorize]
public class InvitationsController : Controller
{
    private readonly IInvitationService _invitationService;

    public InvitationsController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    // GET: Invitations
    public async Task<IActionResult> Index()
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
        
        if (companyId == Guid.Empty)
        {
            return RedirectToAction("Error", "Home");
        }

        var invitations = await _invitationService.GetAllInvitationsByCompanyIdAsync(companyId);
        return View(invitations);
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

    // POST: Invitations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InviteUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty || userId == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Invalid user or company context.");
            return View(model);
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
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        else
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }
    }

    // GET: Invitations/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _invitationService.CancelInvitationAsync(id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Invitations/Accept
    [AllowAnonymous]
    public async Task<IActionResult> Accept(string token, string email)
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
