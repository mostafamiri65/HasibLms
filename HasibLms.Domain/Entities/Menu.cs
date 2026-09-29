namespace HasibLms.Domain.Entities;

public class Menu : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string? Location { get; set; } // header, footer, sidebar
	public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}

public class MenuItem : BaseEntity
{
	public Guid MenuId { get; set; }
	public Menu Menu { get; set; } = null!;
	public string Title { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
	public string? Target { get; set; } // _blank, _self
	public string? Icon { get; set; }
	public int Order { get; set; }
	public Guid? ParentId { get; set; }
	public MenuItem? Parent { get; set; }
	public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
}
