using HasibLms.Domain.Entities.Settings;

namespace HasibLms.Domain.Interfaces;

public interface IWhyChooseUsRepository : IGenericRepository<WhyChooseUsItem>
{
	Task<IReadOnlyList<WhyChooseUsItem>> GetActiveItemsForHomePageAsync(CancellationToken cancellationToken = default);
	Task<bool> ToggleShowSectionAsync(bool show, CancellationToken cancellationToken = default);
	Task<bool> GetShowSectionStatusAsync(CancellationToken cancellationToken = default);
}