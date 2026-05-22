using HasibLms.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

public class ArticleController : Controller
{
	private readonly IArticleService _articleService;
	private readonly ITagService _tagService;
	private readonly ISiteService _siteService;
	private readonly ILogger<ArticleController> _logger;

	public ArticleController(
		IArticleService articleService,
		ITagService tagService,
		ISiteService siteService,
		ILogger<ArticleController> logger)
	{
		_articleService = articleService;
		_tagService = tagService;
		_siteService = siteService;
		_logger = logger;
	}

	[ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page" })]
	public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
	{
		try
		{
			ViewBag.Title = "مقالات آموزشی";
			ViewBag.MetaDescription = "جدیدترین مقالات تخصصی در زمینه حسابداری، مالیات، سرمایه‌گذاری و مدیریت مالی";

			var result = await _articleService.GetPagedArticlesAsync(page, 12, cancellationToken);

			// Get tags for sidebar
			ViewBag.Tags = await _tagService.GetAllTagsAsync(cancellationToken);
			ViewBag.PopularArticles = await _articleService.GetPopularArticlesAsync(5, cancellationToken);

			return View(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading articles page");
			return View("Error");
		}
	}

	[ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "slug" })]
	public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
	{
		try
		{
			if (string.IsNullOrEmpty(slug))
				return NotFound();

			var article = await _articleService.GetArticleBySlugAsync(slug, cancellationToken);
			if (article == null)
				return NotFound();

			ViewBag.Title = article.Title;
			ViewBag.MetaDescription = article.Summary;
			ViewBag.MetaKeywords = string.Join(", ", article.Tags ?? new List<string>());
			ViewBag.OgImage = article.ImageUrl;

			// Get related articles
			var relatedArticles = await _articleService.GetRelatedArticlesAsync(article.Id, 3, cancellationToken);
			ViewBag.RelatedArticles = relatedArticles;

			return View(article);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading article detail for slug: {Slug}", slug);
			return View("Error");
		}
	}

	[ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "slug", "page" })]
	public async Task<IActionResult> ByTag(string slug, int page = 1, CancellationToken cancellationToken = default)
	{
		try
		{
			if (string.IsNullOrEmpty(slug))
				return NotFound();

			var tag = await _tagService.GetTagBySlugAsync(slug, cancellationToken);
			if (tag == null)
				return NotFound();

			ViewBag.Title = $"مقالات تگ {tag.Name}";
			ViewBag.TagName = tag.Name;

			var result = await _articleService.GetArticlesByTagAsync(slug, page, 12, cancellationToken);

			return View(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading articles by tag {Slug}", slug);
			return View("Error");
		}
	}
}