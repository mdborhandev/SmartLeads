using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;
using SmartLeads.Domain.Enums;
using SmartLeads.Infrastructure.Persistence;

namespace SmartLeads.Infrastructure.Services;

public interface ICompanyContext
{
    Guid? CurrentUserId { get; }
    Guid? CurrentCompanyId { get; }
    Guid? CurrentEmployeeId { get; }
    UserCompany? CurrentEmployeeRecord { get; }
    void SetCurrentCompany(Guid companyId);
    Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(CancellationToken token = default);
    Task<Guid?> GetDefaultCompanyIdAsync(CancellationToken token = default);
    void ClearUserCompaniesCache(Guid userId);
    Task<UserRole?> GetCurrentCompanyRoleAsync(CancellationToken token = default);
}

public class CompanyContext : ICompanyContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SmartLeadsDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public CompanyContext(
        IHttpContextAccessor httpContextAccessor,
        SmartLeadsDbContext dbContext,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _cache = cache;
    }

    public Guid? CurrentUserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public Guid? CurrentCompanyId
    {
        get
        {
            var sessionCompanyId = _httpContextAccessor.HttpContext?.Session.GetString("CurrentCompanyId");
            if (!string.IsNullOrEmpty(sessionCompanyId) && Guid.TryParse(sessionCompanyId, out var sessionId))
            {
                return sessionId;
            }

            var cookieCompanyId = _httpContextAccessor.HttpContext?.Request.Cookies["CurrentCompanyId"];
            if (!string.IsNullOrEmpty(cookieCompanyId) && Guid.TryParse(cookieCompanyId, out var cookieId))
            {
                return cookieId;
            }

            var userId = CurrentUserId;
            if (userId.HasValue)
            {
                var defaultCompany = _dbContext.UserCompanies
                    .FirstOrDefault(uc => uc.UserId == userId.Value && uc.IsDefault && uc.IsActive && !uc.IsDeleted);
                return defaultCompany?.CompanyId;
            }

            return null;
        }
    }

    public Guid? CurrentEmployeeId
    {
        get
        {
            var cookieEmployeeId = _httpContextAccessor.HttpContext?.Request.Cookies["CurrentEmployeeId"];
            if (!string.IsNullOrEmpty(cookieEmployeeId) && Guid.TryParse(cookieEmployeeId, out var employeeId))
            {
                return employeeId;
            }
            
            return null;
        }
    }

    public UserCompany? CurrentEmployeeRecord { get; private set; }

    public void SetCurrentCompany(Guid companyId)
    {
        _httpContextAccessor.HttpContext?.Session.SetString("CurrentCompanyId", companyId.ToString());
        CurrentEmployeeRecord = null;
    }

    public async Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(CancellationToken token = default)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
        {
            return Enumerable.Empty<UserCompany>();
        }

        var cacheKey = $"usercompanies_{userId.Value}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<UserCompany>? cached))
        {
            return cached ?? Enumerable.Empty<UserCompany>();
        }

        var userCompanies = await _dbContext.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId.Value && uc.IsActive && !uc.IsDeleted)
            .ToListAsync(token);

        _cache.Set(cacheKey, userCompanies, TimeSpan.FromMinutes(30));

        return userCompanies;
    }
    
    public async Task<Guid?> GetDefaultCompanyIdAsync(CancellationToken token = default)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
        {
            return null;
        }

        var defaultCompany = await _dbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.IsDefault && uc.IsActive && !uc.IsDeleted, token);

        return defaultCompany?.CompanyId;
    }

    public async Task<UserCompany?> GetCurrentEmployeeRecordAsync(CancellationToken token = default)
    {
        if (CurrentEmployeeRecord != null)
        {
            return CurrentEmployeeRecord;
        }

        var userId = CurrentUserId;
        var companyId = CurrentCompanyId;

        if (!userId.HasValue || !companyId.HasValue)
        {
            return null;
        }

        CurrentEmployeeRecord = await _dbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.CompanyId == companyId.Value, token);

        return CurrentEmployeeRecord;
    }
    
    public void ClearUserCompaniesCache(Guid userId)
    {
        var cacheKey = $"usercompanies_{userId}";
        _cache.Remove(cacheKey);
    }
    
    public async Task<UserRole?> GetCurrentCompanyRoleAsync(CancellationToken token = default)
    {
        var userId = CurrentUserId;
        var companyId = CurrentCompanyId;
        
        if (!userId.HasValue || !companyId.HasValue)
        {
            return null;
        }
        
        var userCompany = await _dbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.CompanyId == companyId.Value && uc.IsActive && !uc.IsDeleted, token);
        
        return userCompany?.Role;
    }
}
