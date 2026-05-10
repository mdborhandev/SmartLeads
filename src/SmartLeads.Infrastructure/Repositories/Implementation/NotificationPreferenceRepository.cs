using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class NotificationPreferenceRepository : GenericSystemRepository<NotificationPreference>, INotificationPreferenceRepository
{
    private new readonly SmartLeadsDbContext _dbContext;

    public NotificationPreferenceRepository(SmartLeadsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationPreference?> GetByUserIdAndTypeAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == type, cancellationToken);
    }

    public async Task<IList<NotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationPreferences
            .Where(np => np.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdatePreferenceAsync(Guid userId, NotificationType type, bool enableInApp, bool enableEmail, CancellationToken cancellationToken = default)
    {
        var preference = await GetByUserIdAndTypeAsync(userId, type, cancellationToken);
        
        if (preference == null)
        {
            preference = new NotificationPreference
            {
                UserId = userId,
                NotificationType = type,
                EnableInApp = enableInApp,
                EnableEmail = enableEmail
            };
            await AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.EnableInApp = enableInApp;
            preference.EnableEmail = enableEmail;
            await EditAsync(preference);
        }

        await SaveAsync();
    }

    public async Task InitializeDefaultPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existingPreferences = await GetByUserIdAsync(userId, cancellationToken);
        if (existingPreferences.Any())
        {
            return;
        }

        var preferences = new List<NotificationPreference>();
        foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
        {
            preferences.Add(new NotificationPreference
            {
                UserId = userId,
                NotificationType = type,
                EnableInApp = true,
                EnableEmail = true
            });
        }

        await AddRangeAsync(preferences, cancellationToken);
        await SaveAsync();
    }
}
