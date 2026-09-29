namespace HasibLms.Domain.Entities;

// Domain/Entities/Site.cs
public class Site : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string Domain { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? LogoUrl { get; set; }
	public string? FaviconUrl { get; set; }
	public string? PrimaryColor { get; set; }
	public string? SecondaryColor { get; set; }
	public string? FontFamily { get; set; }
	public bool IsActive { get; set; } = true;
	public ICollection<Page> Pages { get; set; } = new List<Page>();
	public SiteSettings Settings { get; set; } = null!;
}

public class SiteSettings : BaseEntity
{
	public Guid SiteId { get; set; }
	public Site Site { get; set; } = null!;
	public string? HeaderScripts { get; set; }
	public string? FooterScripts { get; set; }
	public string? CssVariables { get; set; }
	public string? CustomCss { get; set; }
	public string? SeoTitle { get; set; }
	public string? SeoDescription { get; set; }
	public string? SeoKeywords { get; set; }
	public bool EnableComments { get; set; }
	public bool EnableReviews { get; set; }
}
