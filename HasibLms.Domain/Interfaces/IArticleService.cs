using HasibLms.Shared.DTOs;

namespace HasibLms.Domain.Interfaces;

public interface IArticleService
{
	Task<ArticleDetailDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task<ArticleDetailDto?> GetArticleByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<PagedResultDto<ArticleCardDto>> GetPagedArticlesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
	Task<PagedResultDto<ArticleCardDto>> GetArticlesByTagAsync(string tagSlug, int page, int pageSize, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<ArticleCardDto>> GetLatestArticlesAsync(int count, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<ArticleCardDto>> GetPopularArticlesAsync(int count, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<ArticleCardDto>> GetRelatedArticlesAsync(Guid articleId, int count, CancellationToken cancellationToken = default);
	Task ClearArticleCacheAsync(Guid? articleId = null);
}