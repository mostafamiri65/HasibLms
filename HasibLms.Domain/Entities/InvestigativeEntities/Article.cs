using HasibLms.Shared.Enumerations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.InvestigativeEntities;

public class Article : BaseEntity
{
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty; // خلاصه برای کارت مقاله
	public string Content { get; set; } = string.Empty; // محتوای HTML کامل
	public string? ImageUrl { get; set; }

	public ArticleType ArticleType { get; set; }
	public bool IsPublished { get; set; }
	public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

	// SEO
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }

	// Foreign Key
	public Guid AuthorId { get; set; }
	[ForeignKey(nameof(AuthorId))]
	public User Author { get; set; } = null!;

	// Tags
	public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}