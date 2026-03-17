using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services.Interface;
using SmartLeads.Utilities.Email;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Infrastructure.Services.Implementation;

public interface IInvitationService
{
    Task<(bool Success, string? Message, Invitation? Invitation)> InviteUserAsync(
        string email, 
        UserRole role, 
        int expiryDays, 
        Guid companyId, 
        Guid invitedByUserId);
    
    Task<(bool Success, string? Message, User? User)> AcceptInvitationAsync(AcceptInvitationRequest request);
    Task<(bool Success, string? Message)> RejectInvitationAsync(Guid invitationId, string? reason = null);
    Task<(bool Success, string? Message)> CancelInvitationAsync(Guid invitationId);
    Task<IList<InvitationDto>> GetPendingInvitationsByCompanyIdAsync(Guid companyId);
    Task<IList<InvitationDto>> GetAllInvitationsByCompanyIdAsync(Guid companyId);
}

public class InvitationService : IInvitationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public InvitationService(
        IUnitOfWork unitOfWork,
        IUserService userService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _emailService = emailService;
    }

    public async Task<(bool Success, string? Message, Invitation? Invitation)> InviteUserAsync(
        string email, 
        UserRole role, 
        int expiryDays, 
        Guid companyId, 
        Guid invitedByUserId)
    {
        try
        {
            // Check if user already exists with this email
            var existingUser = await _userService.GetUserByUsernameOrEmailAsync(email);
            if (existingUser != null)
            {
                return (false, "A user with this email already exists.", null);
            }

            // Check if there's already a pending invitation for this email
            var existingInvitations = await _unitOfWork.invitationRepository.GetByCompanyIdAsync(companyId);
            var existingPendingInvite = existingInvitations
                .FirstOrDefault(i => i.Email.ToLower() == email.ToLower() && i.Status == InvitationStatus.Pending);

            if (existingPendingInvite != null)
            {
                return (false, "An invitation has already been sent to this email.", existingPendingInvite);
            }

            // Create new invitation
            var invitation = new Invitation
            {
                Email = email.ToLower().Trim(),
                Role = role,
                CompanyId = companyId,
                InvitedByUserId = invitedByUserId,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                Status = InvitationStatus.Pending
            };

            await _unitOfWork.invitationRepository.AddAsync(invitation);
            await _unitOfWork.SaveAsync();

            // Send email with invitation link
            try
            {
                var baseUrl = "http://localhost:5000"; // TODO: Get from configuration
                var acceptLink = $"{baseUrl}/Invitations/Accept?token={invitation.Token}&email={Uri.EscapeDataString(invitation.Email)}";
                
                var emailBody = GetInvitationEmailTemplate(invitation.Email, invitation.Role.ToString(), acceptLink, invitation.ExpiresAt);
                
                await _emailService.SendEmailAsync(
                    invitation.Email,
                    "You're Invited to Join SmartLeads!",
                    emailBody
                );
            }
            catch (Exception emailEx)
            {
                // Log email error but don't fail the invitation
                // Invitation is still valid, user can be invited manually
                return (false, $"Invitation created but email failed to send: {emailEx.Message}", invitation);
            }

            return (true, "Invitation sent successfully!", invitation);
        }
        catch (Exception ex)
        {
            return (false, $"Error sending invitation: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string? Message, User? User)> AcceptInvitationAsync(AcceptInvitationRequest request)
    {
        try
        {
            // Find invitation
            var invitation = await _unitOfWork.invitationRepository.GetByEmailAndTokenAsync(request.Email, request.Token);
            
            if (invitation == null)
            {
                return (false, "Invalid invitation token or email.", null);
            }

            // Check if invitation is expired
            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _unitOfWork.invitationRepository.EditAsync(invitation);
                await _unitOfWork.SaveAsync();
                return (false, "This invitation has expired.", null);
            }

            // Check if already accepted
            if (invitation.IsAccepted || invitation.Status == InvitationStatus.Accepted)
            {
                return (false, "This invitation has already been accepted.", null);
            }

            // Check if invitation is cancelled or rejected
            if (invitation.Status == InvitationStatus.Cancelled || invitation.Status == InvitationStatus.Rejected)
            {
                return (false, "This invitation is no longer valid.", null);
            }

            // Create user account with the role from invitation
            var registerResult = await _userService.RegisterAsync(
                request.Email, // Use email as username
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                invitation.CompanyId,
                invitation.Role  // Pass the role from invitation
            );

            if (!registerResult.Success)
            {
                return (false, registerResult.Error, null);
            }

            // Update invitation status
            invitation.IsAccepted = true;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.Status = InvitationStatus.Accepted;
            await _unitOfWork.invitationRepository.EditAsync(invitation);
            await _unitOfWork.SaveAsync();

            return (true, "Invitation accepted successfully! You can now login.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error accepting invitation: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string? Message)> RejectInvitationAsync(Guid invitationId, string? reason = null)
    {
        try
        {
            var invitation = await _unitOfWork.invitationRepository.GetByIdAsync(invitationId);
            
            if (invitation == null)
            {
                return (false, "Invitation not found.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return (false, "This invitation is no longer pending.");
            }

            invitation.Status = InvitationStatus.Rejected;
            invitation.RejectedReason = reason;
            await _unitOfWork.invitationRepository.EditAsync(invitation);

            return (true, "Invitation rejected successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error rejecting invitation: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Message)> CancelInvitationAsync(Guid invitationId)
    {
        try
        {
            var invitation = await _unitOfWork.invitationRepository.GetByIdAsync(invitationId);
            
            if (invitation == null)
            {
                return (false, "Invitation not found.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return (false, "This invitation is no longer pending.");
            }

            invitation.Status = InvitationStatus.Cancelled;
            await _unitOfWork.invitationRepository.EditAsync(invitation);

            return (true, "Invitation cancelled successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error cancelling invitation: {ex.Message}");
        }
    }

    public async Task<IList<InvitationDto>> GetPendingInvitationsByCompanyIdAsync(Guid companyId)
    {
        var invitations = await _unitOfWork.invitationRepository.GetPendingByCompanyIdAsync(companyId);
        return await MapToDtoAsync(invitations);
    }

    public async Task<IList<InvitationDto>> GetAllInvitationsByCompanyIdAsync(Guid companyId)
    {
        var invitations = await _unitOfWork.invitationRepository.GetByCompanyIdAsync(companyId);
        return await MapToDtoAsync(invitations);
    }

    private async Task<IList<InvitationDto>> MapToDtoAsync(IList<Invitation> invitations)
    {
        var dtos = new List<InvitationDto>();
        
        foreach (var invitation in invitations)
        {
            // Get the invited by user's name
            string invitedByUserName = "Unknown";
            if (invitation.InvitedByUserId != Guid.Empty)
            {
                var invitedByUser = await _unitOfWork.userRepository.GetByIdAsync(invitation.InvitedByUserId);
                if (invitedByUser != null)
                {
                    invitedByUserName = !string.IsNullOrEmpty(invitedByUser.FirstName) && !string.IsNullOrEmpty(invitedByUser.LastName)
                        ? $"{invitedByUser.FirstName} {invitedByUser.LastName}"
                        : invitedByUser.Username;
                }
            }
            
            dtos.Add(new InvitationDto
            {
                Id = invitation.Id,
                Email = invitation.Email,
                Role = invitation.Role,
                InvitedByUserName = invitedByUserName,
                InvitedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt,
                IsAccepted = invitation.IsAccepted,
                AcceptedAt = invitation.AcceptedAt,
                Status = invitation.Status.ToString()
            });
        }
        
        return dtos;
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
