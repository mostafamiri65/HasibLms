using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.InvestigativeEntities;

public class ArticleTag
{
	public Guid ArticleId { get; set; }
	[ForeignKey(nameof(ArticleId))]
	public Article Article { get; set; } = null!;

	public Guid TagId { get; set; }
	[ForeignKey(nameof(TagId))]
	public Tag Tag { get; set; } = null!;
}