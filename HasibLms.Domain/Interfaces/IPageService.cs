using HasibLms.Domain.Entities;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Page;

namespace HasibLms.Domain.Interfaces;

public interface IPageService
{
	Task<PageDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task<PageDto?> GetPageForEditAsync(Guid id, CancellationToken cancellationToken = default);
	Task<PagedResultDto<PageDto>> GetPagesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
	Task<AuthResultDto> CreatePageAsync(CreatePageDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdatePageAsync(UpdatePageDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeletePageAsync(Guid id, CancellationToken cancellationToken = default);
	Task<AuthResultDto> AddBlockAsync(Guid pageId, CreatePageBlockDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdateBlockAsync(UpdatePageBlockDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteBlockAsync(Guid blockId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ReorderBlocksAsync(Guid pageId, List<Guid> blockIds, CancellationToken cancellationToken = default);
	Task<HomePageDto> BuildHomePageAsync(CancellationToken cancellationToken = default);
	Task<Dictionary<string, object>> RenderBlockAsync(PageBlock block, CancellationToken cancellationToken = default);
	Task ClearPageCacheAsync(Guid? pageId = null);
}
