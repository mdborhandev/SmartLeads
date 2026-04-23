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

    public async Task<(List<EmployeeDto> data, int total)> GetEmployeesDataAsync(string searchTerm, string sortField, string sortOrder, int page, int pageSize, Guid companyId)
    {
        var query = _defaultDbContext.Employees
            .Where(e => e.CompanyId == companyId && !e.IsDeleted)
            .Include(e => e.EmployeeUsers)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(e =>
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

        var totalCount = query.Count();

        query = sortField?.ToLower() switch
        {
            "employeeid" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.EmployeeId)
                : query.OrderBy(e => e.EmployeeId),
            "department" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.Department.Name)
                : query.OrderBy(e => e.Department.Name),
            "designation" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.Designation.Name)
                : query.OrderBy(e => e.Designation.Name),
            "isactive" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.IsActive)
                : query.OrderBy(e => e.IsActive),
            "createdat" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt),
            _ => query.OrderByDescending(e => e.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return (items, totalCount);
    }
}