namespace HasibLms.Domain.Entities;

// Domain/Entities/PageBlock.cs
public class PageBlock : BaseEntity
{
	public Guid PageId { get; set; }
	public Page Page { get; set; } = null!;

	public string Type { get; set; } = string.Empty; // hero, features, courses, articles, team, testimonials, contact, text, image, video, html, slider, etc.
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Content { get; set; } // می‌تواند HTML یا JSON باشد
	public string? ImageUrl { get; set; }
	public string? BackgroundColor { get; set; }
	public string? TextColor { get; set; }
	public int? Order { get; set; }
	public bool IsActive { get; set; } = true;
	public string? CustomClass { get; set; }
	public string? Settings { get; set; } // JSON برای تنظیمات اضافی

	// برای نمایش محتوای داینامیک (دوره‌ها، مقالات، تیم و ...)
	public string? DataSource { get; set; } // courses, articles, team, etc.
	public int? DataCount { get; set; }
	public string? DataFilter { get; set; } // JSON با فیلترها
}
