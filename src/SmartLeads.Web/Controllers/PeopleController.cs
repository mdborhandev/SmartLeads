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

public class PeopleController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ICompanyContext _companyContext;
    private readonly SmartLeadsDbContext _dbContext;

    public PeopleController(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration,
        ICompanyContext companyContext,
        SmartLeadsDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
        _companyContext = companyContext;
        _dbContext = dbContext;
    }

    // ========================
    // Users
    // ========================

    [Route("~/Users")]
    [Route("~/Users/Index")]
    public async Task<IActionResult> UsersIndex()
    {
        return View("~/Views/Users/Index.cshtml");
    }

    [Route("~/Users/GetUsersData")]
    public async Task<IActionResult> UsersGetData([FromQuery] PaginationRequest request)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty)
        {
            return BadRequest(new { error = "Invalid company context" });
        }

        var result = await _unitOfWork.userRepository.GetUsersPagedAsync(companyId, request);
        return Ok(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Users/Create")]
    public async Task<IActionResult> UsersCreate(CreateUserViewModel model)
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

        var existingUser = await _unitOfWork.userRepository.GetByUsernameAsync(model.Username);
        if (existingUser != null)
        {
            return BadRequest(new { errors = new { Username = new[] { "Username already exists." } } });
        }

        var existingEmail = await _unitOfWork.userRepository.GetByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            return BadRequest(new { errors = new { Email = new[] { "Email already exists." } } });
        }

        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value ?? Guid.Empty.ToString());
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        if (companyId == Guid.Empty || userId == Guid.Empty)
        {
            return BadRequest(new { errors = new { __global = new[] { "Invalid user or company context." } } });
        }

        try
        {
            var existingUserWithEmail = await _unitOfWork.userRepository.GetByEmailAsync(model.Email);
            if (existingUserWithEmail != null)
            {
                return BadRequest(new { errors = new { Email = new[] { "Email already exists." } } });
            }

            var existingPendingInvite = await _unitOfWork.invitationRepository.GetPendingInvitationByEmailAndCompanyIdAsync(model.Email, companyId);
            if (existingPendingInvite != null)
            {
                return BadRequest(new { errors = new { Email = new[] { "An invitation has already been sent to this email." } } });
            }

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
                return BadRequest(new { errors = new { __global = new[] { $"Invitation created but email failed to send: {emailEx.Message}" } } });
            }

            return Ok(new { success = true, message = "Invitation sent successfully! User will be created when they accept the invitation." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new { __global = new[] { $"Error sending invitation: {ex.Message}" } } });
        }
    }

    [Route("~/Users/Edit/{id?}")]
    public async Task<IActionResult> UsersEdit(Guid id)
    {
        var user = await _unitOfWork.userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var companyId = _companyContext.CurrentCompanyId;

        var userRole = UserRole.User;
        if (companyId.HasValue)
        {
            var userCompany = await _dbContext.UserCompanies
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
            EmployeeId = null,
            Department = null,
            Designation = null,
            PhoneNumber = null,
            Address = null,
            DateOfJoining = null,
            IsActive = user.IsActive
        };

        return View("~/Views/Users/Edit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Users/Edit/{id?}")]
    public async Task<IActionResult> UsersEdit(Guid id, EditUserViewModel model)
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

        var companyId = _companyContext.CurrentCompanyId;

        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (companyId.HasValue)
        {
            var userCompany = await _dbContext.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == id && uc.CompanyId == companyId.Value);

            if (userCompany != null)
            {
                userCompany.Role = model.Role;
            }
            else
            {
                userCompany = new UserCompany
                {
                    UserId = id,
                    CompanyId = companyId.Value,
                    Role = model.Role,
                    IsActive = true
                };
                await _dbContext.UserCompanies.AddAsync(userCompany);
            }
        }

        await _unitOfWork.SaveAsync();

        return Ok(new { success = true, message = "User updated successfully!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Users/Delete/{id}")]
    public async Task<IActionResult> UsersDelete(Guid id)
    {
        var user = await _unitOfWork.userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        await _unitOfWork.userRepository.RemoveAsync(id);
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "User deleted successfully!";
        return RedirectToAction("UsersIndex");
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

    // ========================
    // Employees
    // ========================

    [Route("~/Employees")]
    [Route("~/Employees/Index")]
    public async Task<IActionResult> EmployeesIndex()
    {
        return View("~/Views/Employees/Index.cshtml");
    }

    [Route("~/Employees/GetEmployeesData")]
    public async Task<IActionResult> EmployeesGetData([FromQuery] PaginationRequest request)
    {
        try
        {
            Console.WriteLine($"=== GetEmployeesData ===");
            Console.WriteLine($"Page: {request.GetPage()}, PageSize: {request.GetPageSize()}, Search: {request.Search}");

            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            Console.WriteLine($"CompanyIdClaim: {companyIdClaim}");

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { success = false, message = "Invalid company context." });
            }

            var companyId = Guid.Parse(companyIdClaim);
            Console.WriteLine($"Parsed CompanyId: {companyId}");

            var (items, totalCount) = await _unitOfWork.employeeRepository.GetEmployeesDataAsync(
                request.Search ?? "",
                request.GetSortField() ?? "",
                request.GetSortOrder() ?? "",
                request.GetPage(),
                request.GetPageSize(),
                companyId
            );

            Console.WriteLine($"Result: {items.Count} items, Total: {totalCount}");

            return Ok(new
            {
                success = true,
                data = items,
                total = totalCount,
                page = request.GetPage(),
                pageSize = request.GetPageSize()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR ===");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return BadRequest(new { success = false, message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Employees/Save")]
    public async Task<IActionResult> EmployeesSave(EmployeeDto model)
    {
        Console.WriteLine($"Received model: EmployeeId={model.EmployeeId}, FirstName={model.FirstName}, LastName={model.LastName}, Id={model.Id}");

        var errors = new Dictionary<string, List<string>>();
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(companyIdClaim))
        {
            return BadRequest(new { success = false, message = "Invalid company context. Please login again." });
        }

        var companyId = Guid.Parse(companyIdClaim);

        if (string.IsNullOrWhiteSpace(model.EmployeeId))
        {
            errors["EmployeeId"] = new List<string> { "Employee ID is required" };
        }

        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            errors["FirstName"] = new List<string> { "First name is required" };
        }

        if (string.IsNullOrWhiteSpace(model.LastName))
        {
            errors["LastName"] = new List<string> { "Last name is required" };
        }

        if (model.DateOfBirth.HasValue && model.DateOfBirth.Value > DateTime.Now)
        {
            errors["DateOfBirth"] = new List<string> { "Date of birth cannot be in the future." };
        }

        if (model.DateOfJoining.HasValue && model.DateOfJoining.Value > DateTime.Now)
        {
            errors["DateOfJoining"] = new List<string> { "Date of joining cannot be in the future." };
        }

        if (errors.Any())
        {
            return BadRequest(new { success = false, errors, message = "Validation failed. Please check the form." });
        }

        var isEdit = model.Id.HasValue && model.Id.Value != Guid.Empty;

        var employeeRepo = _unitOfWork.employeeRepository;

        if (!isEdit && !string.IsNullOrWhiteSpace(model.EmployeeId))
        {
            var existingByEmployeeId = await employeeRepo.GetByEmployeeIdAsync(model.EmployeeId, companyId);
            if (existingByEmployeeId != null)
            {
                model.Id = existingByEmployeeId.Id;
                isEdit = true;
            }
        }

        try
        {
            if (isEdit)
            {
                var employee = await employeeRepo.GetByIdAsync(model.Id!.Value);
                if (employee == null)
                {
                    return NotFound(new { success = false, message = "Employee not found." });
                }

                if (employee.EmployeeId != model.EmployeeId)
                {
                    var duplicate = await employeeRepo.GetByEmployeeIdExcludingIdAsync(model.EmployeeId, companyId, employee.Id);
                    if (duplicate != null)
                    {
                        return BadRequest(new { success = false, errors = new { EmployeeId = new List<string> { "Employee ID already exists." } }, message = "Employee ID already exists." });
                    }
                }

                employee.EmployeeId = model.EmployeeId;
                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;
                employee.MiddleName = model.MiddleName;
                employee.NickName = model.NickName;
                employee.WorkEmail = model.WorkEmail;
                employee.PersonalEmail = model.PersonalEmail;
                employee.DepartmentId = model.DepartmentId;
                employee.DesignationId = model.DesignationId;
                employee.PhoneNumber = model.PhoneNumber;
                employee.AlternatePhoneNumber = model.AlternatePhoneNumber;
                employee.EmergencyContactName = model.EmergencyContactName;
                employee.EmergencyContactPhone = model.EmergencyContactPhone;
                employee.Address = model.Address;
                employee.DateOfBirth = model.DateOfBirth;
                employee.Gender = model.Gender;
                employee.MaritalStatus = model.MaritalStatus;
                employee.BloodGroup = model.BloodGroup;
                employee.Nationality = model.Nationality;
                employee.NationalIdNumber = model.NationalIdNumber;
                employee.PresentAddress = model.PresentAddress;
                employee.PermanentAddress = model.PermanentAddress;
                employee.JoiningType = model.JoiningType;
                employee.EmploymentStatus = model.EmploymentStatus;
                employee.ProfilePhotoUrl = model.ProfilePhotoUrl;
                employee.Notes = model.Notes;
                employee.DateOfJoining = model.DateOfJoining;
                employee.IsActive = model.IsActive;
                employee.UpdatedAt = DateTime.UtcNow;

                employeeRepo.Edit(employee);
                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, message = "Employee updated successfully!" });
            }
            else
            {
                var existingEmployee = await employeeRepo.GetByEmployeeIdAsync(model.EmployeeId, companyId);
                if (existingEmployee != null)
                {
                    return BadRequest(new { success = false, errors = new { EmployeeId = new List<string> { "Employee ID already exists." } }, message = "Employee ID already exists." });
                }

                var employee = new Employee
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    EmployeeId = model.EmployeeId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    NickName = model.NickName,
                    WorkEmail = model.WorkEmail,
                    PersonalEmail = model.PersonalEmail,
                    DepartmentId = model.DepartmentId,
                    DesignationId = model.DesignationId,
                    PhoneNumber = model.PhoneNumber,
                    AlternatePhoneNumber = model.AlternatePhoneNumber,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    Address = model.Address,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    MaritalStatus = model.MaritalStatus,
                    BloodGroup = model.BloodGroup,
                    Nationality = model.Nationality,
                    NationalIdNumber = model.NationalIdNumber,
                    PresentAddress = model.PresentAddress,
                    PermanentAddress = model.PermanentAddress,
                    JoiningType = model.JoiningType,
                    EmploymentStatus = model.EmploymentStatus,
                    ProfilePhotoUrl = model.ProfilePhotoUrl,
                    Notes = model.Notes,
                    DateOfJoining = model.DateOfJoining,
                    IsActive = true
                };

                await employeeRepo.AddAsync(employee);
                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, message = "Employee created successfully!" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "An error occurred while saving employee.", details = ex.Message });
        }
    }

    [Route("~/Employees/GetEmployee/{id}")]
    public async Task<IActionResult> EmployeesGet(Guid id)
    {
        var employee = await _unitOfWork.employeeRepository.GetByEmployeeDtoByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { success = false, message = "Employee not found." });
        }

        var model = new EmployeeDto
        {
            Id = employee.Id,
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            MiddleName = employee.MiddleName,
            NickName = employee.NickName,
            WorkEmail = employee.WorkEmail,
            PersonalEmail = employee.PersonalEmail,
            DepartmentId = employee.DepartmentId,
            DesignationId = employee.DesignationId,
            DepartmentName = employee.Department?.Name,
            DesignationName = employee.Designation?.Name,
            PhoneNumber = employee.PhoneNumber,
            AlternatePhoneNumber = employee.AlternatePhoneNumber,
            EmergencyContactName = employee.EmergencyContactName,
            EmergencyContactPhone = employee.EmergencyContactPhone,
            Address = employee.Address,
            PresentAddress = employee.PresentAddress,
            PermanentAddress = employee.PermanentAddress,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender,
            MaritalStatus = employee.MaritalStatus,
            BloodGroup = employee.BloodGroup,
            Nationality = employee.Nationality,
            NationalIdNumber = employee.NationalIdNumber,
            DateOfJoining = employee.DateOfJoining,
            JoiningType = employee.JoiningType,
            EmploymentStatus = employee.EmploymentStatus,
            ProfilePhotoUrl = employee.ProfilePhotoUrl,
            Notes = employee.Notes,
            IsActive = employee.IsActive
        };

        return Ok(new { success = true, data = model });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("~/Employees/Delete/{id}")]
    public async Task<IActionResult> EmployeesDelete(Guid id)
    {
        var employee = await _unitOfWork.employeeRepository.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { success = false, message = "Employee not found." });
        }

        await _unitOfWork.employeeRepository.SoftDeleteAsync(id);
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "Employee deleted successfully!";
        return RedirectToAction("EmployeesIndex");
    }
}
