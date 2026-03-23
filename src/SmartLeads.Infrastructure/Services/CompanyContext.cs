using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;

namespace SmartLeads.Infrastructure.Services;

/// <summary>
/// Service for managing the current user's company context.
/// Users can belong to multiple companies, so we need to track which company they're currently working with.
/// </summary>
public interface ICompanyContext
{
    /// <summary>
    /// The current user's ID.
    /// </summary>
    Guid? CurrentUserId { get; }

    /// <summary>
    /// The current company ID the user is working with.
    /// </summary>
    Guid? CurrentCompanyId { get; }

    /// <summary>
    /// The current user's employee information for the current company.
    /// </summary>
    UserCompany? CurrentEmployeeRecord { get; }

    /// <summary>
    /// Sets the current company context for the user.
    /// </summary>
    void SetCurrentCompany(Guid companyId);

    /// <summary>
    /// Gets all companies the current user belongs to.
    /// </summary>
    Task<IEnumerable<UserCompany>> GetUserCompaniesAsync(CancellationToken token = default);
}

public class CompanyContext : ICompanyContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SystemDbContext _systemDbContext;
    private readonly IMemoryCache _cache;

    public CompanyContext(
        IHttpContextAccessor httpContextAccessor,
        SystemDbContext systemDbContext,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _systemDbContext = systemDbContext;
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

    public Guid? CurrentCompanyId { get; private set; }

    public UserCompany? CurrentEmployeeRecord { get; private set; }

    public void SetCurrentCompany(Guid companyId)
    {
        CurrentCompanyId = companyId;
        CurrentEmployeeRecord = null; // Will be loaded on demand
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

        var userCompanies = await _systemDbContext.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId.Value && uc.IsActive && !uc.IsDeleted)
            .ToListAsync(token);

        _cache.Set(cacheKey, userCompanies, TimeSpan.FromMinutes(30));

        return userCompanies;
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

        CurrentEmployeeRecord = await _systemDbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId.Value && uc.CompanyId == companyId.Value, token);

        return CurrentEmployeeRecord;
    }
}
