using HasibLms.Domain.Entities.InvestigativeEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class ArticleService : IArticleService
{
	private readonly IArticleRepository _articleRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<ArticleService> _logger;

	public ArticleService(
		IArticleRepository articleRepository,
		IMemoryCache cache,
		ILogger<ArticleService> logger)
	{
		_articleRepository = articleRepository;
		_cache = cache;
		_logger = logger;
	}

	public async Task<ArticleDetailDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"article_slug_{slug}";

			if (_cache.TryGetValue(cacheKey, out ArticleDetailDto? cached) && cached != null)
				return cached;

			var article = await _articleRepository.GetBySlugAsync(slug, cancellationToken);
			if (article == null) return null;

			var result = new ArticleDetailDto
			{
				Id = article.Id,
				Title = article.Title,
				Slug = article.Slug,
				Summary = article.Summary,
				Content = article.Content,
				ImageUrl = article.ImageUrl,
				PublishedAt = article.PublishedAt,
				AuthorName = article.Author?.FullName ?? "نویسنده",
				AuthorAvatar = article.Author?.AvatarUrl,
				ArticleType = article.ArticleType.ToString(),
				MetaTitle = article.MetaTitle,
				MetaDescription = article.MetaDescription,
				Tags = article.ArticleTags?.Select(at => at.Tag.Name).ToList() ?? new List<string>(),
				ViewCount = 0 // TODO: Implement view count
			};

			// Get related articles
			result.RelatedArticles = (await GetRelatedArticlesAsync(article.Id, 3, cancellationToken)).ToList();

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting article by slug: {Slug}", slug);
			return null;
		}
	}

	public async Task<ArticleDetailDto?> GetArticleByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"article_id_{id}";

			if (_cache.TryGetValue(cacheKey, out ArticleDetailDto? cached) && cached != null)
				return cached;

			var article = await _articleRepository.GetArticleWithDetailsAsync(id, cancellationToken);
			if (article == null) return null;

			var result = new ArticleDetailDto
			{
				Id = article.Id,
				Title = article.Title,
				Slug = article.Slug,
				Summary = article.Summary,
				Content = article.Content,
				ImageUrl = article.ImageUrl,
				PublishedAt = article.PublishedAt,
				AuthorName = article.Author?.FullName ?? "نویسنده",
				AuthorAvatar = article.Author?.AvatarUrl,
				ArticleType = article.ArticleType.ToString(),
				MetaTitle = article.MetaTitle,
				MetaDescription = article.MetaDescription,
				Tags = article.ArticleTags?.Select(at => at.Tag.Name).ToList() ?? new List<string>(),
				ViewCount = 0
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting article by id: {Id}", id);
			return null;
		}
	}

	public async Task<PagedResultDto<ArticleCardDto>> GetPagedArticlesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
	{
		try
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

			var cacheKey = $"articles_page_{page}_{pageSize}";

			if (_cache.TryGetValue(cacheKey, out PagedResultDto<ArticleCardDto>? cached) && cached != null)
				return cached;

			var articles = await _articleRepository.GetPagedArticlesAsync(page, pageSize, cancellationToken);
			var totalCount = await _articleRepository.CountAsync(a => a.IsPublished, cancellationToken);

			var result = new PagedResultDto<ArticleCardDto>
			{
				Items = articles.Select(MapToCardDto).ToList(),
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = pageSize
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
				SlidingExpiration = TimeSpan.FromMinutes(1)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting paged articles");
			return new PagedResultDto<ArticleCardDto>
			{
				Items = new List<ArticleCardDto>(),
				TotalCount = 0,
				PageNumber = page,
				PageSize = pageSize
			};
		}
	}

	public async Task<PagedResultDto<ArticleCardDto>> GetArticlesByTagAsync(string tagSlug, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		try
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

			var cacheKey = $"articles_tag_{tagSlug}_{page}_{pageSize}";

			if (_cache.TryGetValue(cacheKey, out PagedResultDto<ArticleCardDto>? cached) && cached != null)
				return cached;

			var articles = await _articleRepository.GetArticlesByTagAsync(tagSlug, page, pageSize, cancellationToken);
			var totalCount = await _articleRepository.CountAsync(a => a.IsPublished && a.ArticleTags.Any(at => at.Tag.Slug == tagSlug), cancellationToken);

			var result = new PagedResultDto<ArticleCardDto>
			{
				Items = articles.Select(MapToCardDto).ToList(),
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = pageSize
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
				SlidingExpiration = TimeSpan.FromMinutes(1)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting articles by tag: {TagSlug}", tagSlug);
			return new PagedResultDto<ArticleCardDto>
			{
				Items = new List<ArticleCardDto>(),
				TotalCount = 0,
				PageNumber = page,
				PageSize = pageSize
			};
		}
	}

	public async Task<IReadOnlyList<ArticleCardDto>> GetLatestArticlesAsync(int count, CancellationToken cancellationToken = default)
	{
		try
		{
			count = Math.Clamp(count, 1, 50);
			var cacheKey = $"latest_articles_{count}";

			if (_cache.TryGetValue(cacheKey, out List<ArticleCardDto>? cached) && cached != null)
				return cached;

			var articles = await _articleRepository.GetLatestPublishedAsync(count, cancellationToken);
			var result = articles.Select(MapToCardDto).ToList();

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting latest articles");
			return new List<ArticleCardDto>();
		}
	}

	public async Task<IReadOnlyList<ArticleCardDto>> GetPopularArticlesAsync(int count, CancellationToken cancellationToken = default)
	{
		try
		{
			count = Math.Clamp(count, 1, 50);
			var cacheKey = $"popular_articles_{count}";

			if (_cache.TryGetValue(cacheKey, out List<ArticleCardDto>? cached) && cached != null)
				return cached;

			// TODO: Implement based on view count or comments
			var articles = await _articleRepository.GetLatestPublishedAsync(count, cancellationToken);
			var result = articles.Select(MapToCardDto).ToList();

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
				SlidingExpiration = TimeSpan.FromMinutes(15)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting popular articles");
			return new List<ArticleCardDto>();
		}
	}

	public async Task<IReadOnlyList<ArticleCardDto>> GetRelatedArticlesAsync(Guid articleId, int count, CancellationToken cancellationToken = default)
	{
		try
		{
			count = Math.Clamp(count, 1, 10);
			var cacheKey = $"related_articles_{articleId}_{count}";

			if (_cache.TryGetValue(cacheKey, out List<ArticleCardDto>? cached) && cached != null)
				return cached;

			var currentArticle = await _articleRepository.GetByIdAsync(articleId, cancellationToken);
			if (currentArticle == null) return new List<ArticleCardDto>();

			var result = new List<ArticleCardDto>();
			var addedIds = new HashSet<Guid> { articleId };

			// Get articles with same tags
			if (currentArticle.ArticleTags != null && currentArticle.ArticleTags.Any())
			{
				var tagIds = currentArticle.ArticleTags.Select(at => at.TagId).ToList();
				var related = await _articleRepository.GetAsync(a =>
					a.IsPublished &&
					a.Id != articleId &&
					a.ArticleTags.Any(at => tagIds.Contains(at.TagId)), cancellationToken);

				foreach (var article in related.Take(count))
				{
					if (!addedIds.Contains(article.Id))
					{
						result.Add(MapToCardDto(article));
						addedIds.Add(article.Id);
					}
				}
			}

			// Fallback to latest articles
			if (result.Count < count)
			{
				var latest = await _articleRepository.GetAsync(a => a.IsPublished && !addedIds.Contains(a.Id), cancellationToken);
				foreach (var article in latest.OrderByDescending(a => a.PublishedAt).Take(count - result.Count))
				{
					result.Add(MapToCardDto(article));
				}
			}

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting related articles for {ArticleId}", articleId);
			return new List<ArticleCardDto>();
		}
	}

	public async Task ClearArticleCacheAsync(Guid? articleId = null)
	{
		try
		{
			if (articleId.HasValue)
			{
				_cache.Remove($"article_id_{articleId.Value}");
				_cache.Remove($"article_slug_*");
			}

			_cache.Remove("latest_articles_*");
			_cache.Remove("popular_articles_*");
			_cache.Remove("articles_page_*");
			_cache.Remove("articles_tag_*");

			_logger.LogInformation("Article cache cleared for {ArticleId}", articleId ?? Guid.Empty);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error clearing article cache");
		}
	}

	private ArticleCardDto MapToCardDto(Article article)
	{
		return new ArticleCardDto
		{
			Id = article.Id,
			Title = article.Title,
			Slug = article.Slug,
			Summary = article.Summary,
			ImageUrl = article.ImageUrl,
			PublishedAt = article.PublishedAt,
			AuthorName = article.Author?.FullName ?? "نویسنده",
			AuthorAvatar = article.Author?.AvatarUrl,
			Tags = article.ArticleTags?.Select(at => at.Tag.Name).ToList() ?? new List<string>(),
			ViewCount = 0,
			IsFeatured = article.FeatureStatus
		};
	}
}