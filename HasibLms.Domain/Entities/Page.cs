
using HasibLms.Domain.Entities;

public class Page : BaseEntity
{
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? MetaKeywords { get; set; }
	public string? CanonicalUrl { get; set; }
	public bool IsPublished { get; set; } = true;
	public DateTime? PublishedAt { get; set; }
	public int? Order { get; set; }
	public string? Template { get; set; } // نام قالب
	public bool IsHomePage { get; set; }
	public bool ShowInMenu { get; set; }
	public Guid? ParentId { get; set; }
	public Page? Parent { get; set; }
	public ICollection<Page> Children { get; set; } = new List<Page>();
	public ICollection<PageBlock> Blocks { get; set; } = new List<PageBlock>();
}