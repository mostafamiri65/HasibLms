// IAdminArticleService.cs
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;

namespace HasibLms.Domain.Interfaces;

public interface IAdminArticleService
{
    // مدیریت مقالات
    Task<PagedResultDto<ArticleListAdminDto>> GetArticlesAsync(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<ArticleDetailAdminDto?> GetArticleForEditAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuthResultDto> CreateArticleAsync(CreateArticleAdminDto model,string rootPath, Guid authorId, CancellationToken cancellationToken = default);
    Task<AuthResultDto> UpdateArticleAsync(UpdateArticleAdminDto model, string rootPath, CancellationToken cancellationToken = default);
    Task<AuthResultDto> DeleteArticleAsync(Guid id, string rootPath, CancellationToken cancellationToken = default);
    Task<AuthResultDto> TogglePublishStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuthResultDto> ToggleFeatureStatusAsync(Guid id, bool isFeatured, CancellationToken cancellationToken = default);

    // مدیریت تگ‌ها
    Task<List<TagListDto>> GetAllTagsAsync(CancellationToken cancellationToken = default);
    Task<AuthResultDto> CreateTagAsync(CreateTagDto model, CancellationToken cancellationToken = default);
    Task<AuthResultDto> UpdateTagAsync(UpdateTagDto model, CancellationToken cancellationToken = default);
    Task<AuthResultDto> DeleteTagAsync(Guid id, CancellationToken cancellationToken = default);
}