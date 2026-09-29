using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Settings;

namespace HasibLms.Domain.Interfaces;

public interface IWhyChooseUsService
{
	Task<WhyChooseUsSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
	Task<List<WhyChooseUsItemDto>> GetActiveItemsForHomePageAsync(CancellationToken cancellationToken = default);
	Task<AuthResultDto> CreateItemAsync(CreateWhyChooseUsItemDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdateItemAsync(UpdateWhyChooseUsItemDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ReorderItemsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ToggleShowSectionAsync(bool show, CancellationToken cancellationToken = default);
	Task<WhyChooseUsItemDto?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
}