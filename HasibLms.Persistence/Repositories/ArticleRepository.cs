using HasibLms.Domain.Entities.InvestigativeEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class ArticleRepository : GenericRepository<Article>, IArticleRepository
{
	public ArticleRepository(LmsContext context) : base(context)
	{
	}

	public async Task<Article?> GetArticleWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(a => a.Author)
			.Include(a => a.ArticleTags)
				.ThenInclude(at => at.Tag)
			.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted && a.IsPublished, cancellationToken);
	}

	public async Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(a => a.Author)
			.Include(a => a.ArticleTags)
				.ThenInclude(at => at.Tag)
			.FirstOrDefaultAsync(a => a.Slug == slug && !a.IsDeleted && a.IsPublished, cancellationToken);
	}

	public async Task<IReadOnlyList<Article>> GetLatestPublishedAsync(int count, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(a => !a.IsDeleted && a.IsPublished)
			.Include(a => a.Author)
			.OrderByDescending(a => a.PublishedAt)
			.Take(count)
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<Article>> GetPagedArticlesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(a => !a.IsDeleted && a.IsPublished)
			.Include(a => a.Author)
			.OrderByDescending(a => a.PublishedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<Article>> GetArticlesByTagAsync(string tagSlug, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(a => !a.IsDeleted && a.IsPublished && a.ArticleTags.Any(at => at.Tag.Slug == tagSlug))
			.Include(a => a.Author)
			.OrderByDescending(a => a.PublishedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}
	public async Task<IReadOnlyList<Article>> GetPagedArticlesAsync(
		int page,
		int pageSize,
		string? searchTerm = null,
		string? tagSlug = null,
		CancellationToken cancellationToken = default)
	{
		IQueryable<Article> query = _dbSet
			.Where(a => !a.IsDeleted && a.IsPublished)
			.Include(a => a.Author);

		// فیلتر بر اساس جستجو
		if (!string.IsNullOrEmpty(searchTerm))
		{
			searchTerm = searchTerm.Trim();
			query = query.Where(a =>
				a.Title.Contains(searchTerm) ||
				a.Summary.Contains(searchTerm) ||
				a.Content.Contains(searchTerm));
		}

		// فیلتر بر اساس تگ
		if (!string.IsNullOrEmpty(tagSlug))
		{
			query = query.Where(a => a.ArticleTags.Any(at => at.Tag.Slug == tagSlug));
		}

		return await query
			.OrderByDescending(a => a.PublishedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}
}