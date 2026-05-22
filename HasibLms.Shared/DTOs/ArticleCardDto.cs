namespace HasibLms.Shared.DTOs;

public class ArticleCardDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty;
	public string? ImageUrl { get; set; }
	public DateTime PublishedAt { get; set; }
	public string AuthorName { get; set; } = string.Empty;
	public string? AuthorAvatar { get; set; }
	public int ViewCount { get; set; }
	public List<string> Tags { get; set; } = new();
}

public class ArticleDetailDto : ArticleCardDto
{
	public string Content { get; set; } = string.Empty;
	public string ArticleType { get; set; } = string.Empty;
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public List<ArticleCardDto> RelatedArticles { get; set; } = new();
}