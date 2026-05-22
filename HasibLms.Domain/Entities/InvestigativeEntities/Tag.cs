namespace HasibLms.Domain.Entities.InvestigativeEntities;

public class Tag : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;

	public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}