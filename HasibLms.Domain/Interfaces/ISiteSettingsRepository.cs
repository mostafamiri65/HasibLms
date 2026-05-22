using HasibLms.Domain.Entities.Settings;

namespace HasibLms.Domain.Interfaces;

public interface ISiteSettingsRepository : IGenericRepository<SiteSetting>
{
	Task<SiteSetting?> GetSettingsAsync(CancellationToken cancellationToken = default);
	Task<bool> UpdateSettingsAsync(SiteSetting settings, CancellationToken cancellationToken = default);
}