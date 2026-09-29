using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.Settings;

public class MenuItemEntity : BaseEntity
{
	public string Title { get; set; } = string.Empty;
	public string? Url { get; set; }
	public string? Icon { get; set; }
	public int Order { get; set; }
	public bool IsActive { get; set; } = true;
	public string? Target { get; set; } // _blank, _self
	public string? CssClass { get; set; }

	// برای منوی فرزند
	public Guid? ParentId { get; set; }
	[ForeignKey(nameof(ParentId))]
	public MenuItemEntity? Parent { get; set; }

	// موقعیت منو (header, footer, sidebar)
	public string Location { get; set; } = "header";

	public ICollection<MenuItemEntity> Children { get; set; } = new List<MenuItemEntity>();
}