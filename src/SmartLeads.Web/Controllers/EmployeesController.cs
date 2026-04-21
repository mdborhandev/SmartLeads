using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Domain.Models;

namespace SmartLeads.Web.Controllers;

public class EmployeesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeesData([FromQuery] PaginationRequest request)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            
            if (string.IsNullOrEmpty(companyIdClaim))
            {
                return BadRequest(new { success = false, message = "Invalid company context." });
            }

            var companyId = Guid.Parse(companyIdClaim);

            if (companyId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "Invalid company context." });
            }

            var employees = _unitOfWork.defaultDbContext.Employees
                .Where(e => e.CompanyId == companyId && !e.IsDeleted)
                .Include(e => e.EmployeeUsers)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                employees = employees.Where(e =>
                    e.EmployeeId.ToLower().Contains(search) ||
                    e.FirstName.ToLower().Contains(search) ||
                    e.LastName.ToLower().Contains(search) ||
                    ((e.NickName ?? string.Empty).ToLower().Contains(search)) ||
                    ((e.WorkEmail ?? string.Empty).ToLower().Contains(search)) ||
                    ((e.PersonalEmail ?? string.Empty).ToLower().Contains(search)) ||
                    (e.Department != null && e.Department.Name.ToLower().Contains(search)) ||
                    (e.Designation != null && e.Designation.Name.ToLower().Contains(search)) ||
                    (e.PhoneNumber != null && e.PhoneNumber.ToLower().Contains(search))
                );
            }

            var totalCount = employees.Count();

            employees = request.SortField?.ToLower() switch
            {
                "employeeid" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.EmployeeId)
                    : employees.OrderBy(e => e.EmployeeId),
                "department" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.Department.Name)
                    : employees.OrderBy(e => e.Department.Name),
                "designation" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.Designation.Name)
                    : employees.OrderBy(e => e.Designation.Name),
                "isactive" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.IsActive)
                    : employees.OrderBy(e => e.IsActive),
                "createdat" => request.SortOrder?.ToLower() == "desc"
                    ? employees.OrderByDescending(e => e.CreatedAt)
                    : employees.OrderBy(e => e.CreatedAt),
                _ => employees.OrderByDescending(e => e.CreatedAt)
            };

            var items = await employees
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    MiddleName = e.MiddleName,
                    NickName = e.NickName,
                    WorkEmail = e.WorkEmail,
                    PersonalEmail = e.PersonalEmail,
                    DepartmentId = e.DepartmentId,
                    DesignationId = e.DesignationId,
                    Department = e.Department != null ? e.Department.Name : null,
                    Designation = e.Designation != null ? e.Designation.Name : null,
                    PhoneNumber = e.PhoneNumber,
                    AlternatePhoneNumber = e.AlternatePhoneNumber,
                    EmergencyContactName = e.EmergencyContactName,
                    EmergencyContactPhone = e.EmergencyContactPhone,
                    Address = e.Address,
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt,
                    DateOfBirth = e.DateOfBirth,
                    Gender = e.Gender,
                    MaritalStatus = e.MaritalStatus,
                    BloodGroup = e.BloodGroup,
                    Nationality = e.Nationality,
                    NationalIdNumber = e.NationalIdNumber,
                    PresentAddress = e.PresentAddress,
                    PermanentAddress = e.PermanentAddress,
                    DateOfJoining = e.DateOfJoining,
                    JoiningType = e.JoiningType,
                    EmploymentStatus = e.EmploymentStatus,
                    ProfilePhotoUrl = e.ProfilePhotoUrl,
                    Notes = e.Notes
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = items,
                total = totalCount,
                page = request.Page,
                pageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "Failed to get employees.", details = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(EmployeeDto model)
    {
        // Log received data for debugging
        Console.WriteLine($"Received model: EmployeeId={model.EmployeeId}, FirstName={model.FirstName}, LastName={model.LastName}, Id={model.Id}");

        var errors = new Dictionary<string, List<string>>();
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(companyIdClaim))
        {
            return BadRequest(new { success = false, message = "Invalid company context. Please login again." });
        }

        var companyId = Guid.Parse(companyIdClaim);

        // Manual validation - check required fields
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

        try
        {
            var employeeRepo = _unitOfWork.employeeRepository;

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

    [HttpGet]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        var employee = await _unitOfWork.employeeRepository.GetByIdAsync(id);
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
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _unitOfWork.employeeRepository.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { success = false, message = "Employee not found." });
        }

        await _unitOfWork.employeeRepository.SoftDeleteAsync(id);
        await _unitOfWork.SaveAsync();

        TempData["SuccessMessage"] = "Employee deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}