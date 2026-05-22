using HasibLms.Shared.DTOs;

namespace HasibLms.Domain.Interfaces;

public interface ITagService
{
	Task<List<TagDto>> GetAllTagsAsync(CancellationToken cancellationToken = default);
	Task<TagDto?> GetTagBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task<TagDto?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken = default);
}