using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(DefaultDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Employee?> GetByEmployeeIdAsync(string employeeId, Guid companyId)
    {
        return await _defaultDbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);
    }

    public async Task<Employee?> GetByEmployeeIdExcludingIdAsync(string employeeId, Guid companyId, Guid excludeId)
    {
        return await _defaultDbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && e.Id != excludeId && !e.IsDeleted);
    }

    public async Task<Employee?> GetByEmployeeDtoByIdAsync(Guid id)
    {
        return await _defaultDbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<(List<EmployeeDto> data, int total)> GetEmployeesDataAsync(string searchTerm, string sortField, string sortOrder, int page, int pageSize, Guid companyId)
    {
        var query = _defaultDbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Where(e => e.CompanyId == companyId && !e.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(e =>
                e.EmployeeId.ToLower().Contains(search) ||
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                ((e.NickName ?? string.Empty).ToLower().Contains(search))
            );
        }

        var totalCount = query.Count();

        query = sortField?.ToLower() switch
        {
            "employeeid" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.EmployeeId)
                : query.OrderBy(e => e.EmployeeId),
            "isactive" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.IsActive)
                : query.OrderBy(e => e.IsActive),
            "createdat" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt),
            _ => query.OrderByDescending(e => e.CreatedAt)
        };

        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get all variable values from employees
        var allVariableValues = employees
            .Where(e => e.Gender != null).Select(e => e.Gender!)
            .Concat(employees.Where(e => e.MaritalStatus != null).Select(e => e.MaritalStatus!))
            .Concat(employees.Where(e => e.BloodGroup != null).Select(e => e.BloodGroup!))
            .Concat(employees.Where(e => e.Nationality != null).Select(e => e.Nationality!))
            .Concat(employees.Where(e => e.JoiningType != null).Select(e => e.JoiningType!))
            .Concat(employees.Where(e => e.EmploymentStatus != null).Select(e => e.EmploymentStatus!))
            .Distinct()
            .ToList();

        // Fetch variables where Value matches (the stored value is the ID, but for backward compatibility we match by Value too)
        var variables = await _defaultDbContext.Variables
            .Where(v => allVariableValues.Contains(v.Value) && v.CompanyId == companyId)
            .ToDictionaryAsync(v => v.Value, v => v.Value);

        // Also fetch by Id for GUID-based storage
        var variablesById = await _defaultDbContext.Variables
            .Where(v => allVariableValues.Contains(v.Id.ToString()) && v.CompanyId == companyId)
            .ToDictionaryAsync(v => v.Id.ToString(), v => v.Value);

        // Convert to DTO with display text
        var items = employees.Select(e => new EmployeeDto
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
            DepartmentName = e.Department?.Name,
            DesignationName = e.Designation?.Name,
            PhoneNumber = e.PhoneNumber,
            AlternatePhoneNumber = e.AlternatePhoneNumber,
            EmergencyContactName = e.EmergencyContactName,
            EmergencyContactPhone = e.EmergencyContactPhone,
            Address = e.Address,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt,
            DateOfBirth = e.DateOfBirth,
            // Store original values
            Gender = e.Gender,
            MaritalStatus = e.MaritalStatus,
            BloodGroup = e.BloodGroup,
            Nationality = e.Nationality,
            JoiningType = e.JoiningType,
            EmploymentStatus = e.EmploymentStatus,
            NationalIdNumber = e.NationalIdNumber,
            PresentAddress = e.PresentAddress,
            PermanentAddress = e.PermanentAddress,
            DateOfJoining = e.DateOfJoining,
            ProfilePhotoUrl = e.ProfilePhotoUrl,
            Notes = e.Notes,
            // Get display text - first try by Id, then by Value
            GenderText = e.Gender != null ? (variablesById.ContainsKey(e.Gender) ? variablesById[e.Gender] : (variables.ContainsKey(e.Gender) ? variables[e.Gender] : e.Gender)) : null,
            MaritalStatusText = e.MaritalStatus != null ? (variablesById.ContainsKey(e.MaritalStatus) ? variablesById[e.MaritalStatus] : (variables.ContainsKey(e.MaritalStatus) ? variables[e.MaritalStatus] : e.MaritalStatus)) : null,
            BloodGroupText = e.BloodGroup != null ? (variablesById.ContainsKey(e.BloodGroup) ? variablesById[e.BloodGroup] : (variables.ContainsKey(e.BloodGroup) ? variables[e.BloodGroup] : e.BloodGroup)) : null,
            NationalityText = e.Nationality != null ? (variablesById.ContainsKey(e.Nationality) ? variablesById[e.Nationality] : (variables.ContainsKey(e.Nationality) ? variables[e.Nationality] : e.Nationality)) : null,
            JoiningTypeText = e.JoiningType != null ? (variablesById.ContainsKey(e.JoiningType) ? variablesById[e.JoiningType] : (variables.ContainsKey(e.JoiningType) ? variables[e.JoiningType] : e.JoiningType)) : null,
            EmploymentStatusText = e.EmploymentStatus != null ? (variablesById.ContainsKey(e.EmploymentStatus) ? variablesById[e.EmploymentStatus] : (variables.ContainsKey(e.EmploymentStatus) ? variables[e.EmploymentStatus] : e.EmploymentStatus)) : null
        }).ToList();

        return (items, totalCount);
    }
}