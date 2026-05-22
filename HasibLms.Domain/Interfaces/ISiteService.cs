using HasibLms.Domain.Entities.Settings;
using HasibLms.Shared.DTOs.Settings;

namespace HasibLms.Domain.Interfaces;

public interface ISiteService
{
	Task<SiteSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default);
	Task<SiteSettingsDto?> UpdateSettingsAsync(UpdateSiteSettingsDto model, Guid userId, CancellationToken cancellationToken = default);
	Task<string> GetInstituteNameAsync();
	Task<string> GetSloganAsync();
}