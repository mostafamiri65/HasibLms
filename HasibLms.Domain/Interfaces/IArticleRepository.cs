using HasibLms.Domain.Entities.InvestigativeEntities;

namespace HasibLms.Domain.Interfaces;

public interface IArticleRepository : IGenericRepository<Article>
{
	Task<Article?> GetArticleWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
	Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Article>> GetLatestPublishedAsync(int count, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Article>> GetPagedArticlesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Article>> GetArticlesByTagAsync(string tagSlug, int page, int pageSize, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Article>> GetPagedArticlesAsync(
		int page,
		int pageSize,
		string? searchTerm = null,
		string? tagSlug = null,
		CancellationToken cancellationToken = default);
}