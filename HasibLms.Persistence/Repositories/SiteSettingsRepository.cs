using HasibLms.Domain.Entities.Settings;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class SiteSettingsRepository : GenericRepository<SiteSetting>, ISiteSettingsRepository
{
	public SiteSettingsRepository(LmsContext context) : base(context)
	{
	}

	public async Task<SiteSetting?> GetSettingsAsync(CancellationToken cancellationToken = default)
	{
		// فقط یک رکورد تنظیمات وجود دارد
		return await _dbSet.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<bool> UpdateSettingsAsync(SiteSetting settings, CancellationToken cancellationToken = default)
	{
		settings.LastUpdated = DateTime.UtcNow;
		_dbSet.Update(settings);
		var result = await _context.SaveChangesAsync(cancellationToken);
		return result > 0;
	}
}