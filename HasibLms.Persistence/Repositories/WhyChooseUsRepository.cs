using HasibLms.Domain.Entities.Settings;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class WhyChooseUsRepository : GenericRepository<WhyChooseUsItem>, IWhyChooseUsRepository
{
	private readonly LmsContext _context;

	public WhyChooseUsRepository(LmsContext context) : base(context)
	{
		_context = context;
	}

	public async Task<IReadOnlyList<WhyChooseUsItem>> GetActiveItemsForHomePageAsync(CancellationToken cancellationToken = default)
	{
		var showSection = await GetShowSectionStatusAsync(cancellationToken);
		if (!showSection) return new List<WhyChooseUsItem>();

		return await _dbSet
			.Where(w => w.IsActive && w.ShowOnHomePage && !w.IsDeleted)
			.OrderBy(w => w.Order)
			.ToListAsync(cancellationToken);
	}

	public async Task<bool> ToggleShowSectionAsync(bool show, CancellationToken cancellationToken = default)
	{
		// این تنظیمات را در SiteSetting ذخیره می‌کنیم یا یک جدول جداگانه
		// فعلاً از SiteSetting استفاده می‌کنیم
		var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken);
		if (settings != null)
		{
			// یک فیلد جدید به SiteSetting اضافه کنید
			// settings.ShowWhyChooseUs = show;
			// await _context.SaveChangesAsync(cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> GetShowSectionStatusAsync(CancellationToken cancellationToken = default)
	{
		var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken);
		// پیش‌فرض true
		return settings != null && settings.IsSiteActive;
	}
}