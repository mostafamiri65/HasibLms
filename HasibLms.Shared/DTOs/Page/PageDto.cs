namespace HasibLms.Shared.DTOs.Page;

public class PageDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? MetaKeywords { get; set; }
	public bool IsPublished { get; set; }
	public bool IsHomePage { get; set; }
	public bool ShowInMenu { get; set; }
	public string? Template { get; set; }
	public List<PageBlockDto> Blocks { get; set; } = new();
	public List<PageDto> Children { get; set; } = new();
}

public class PageBlockDto
{
	public Guid Id { get; set; }
	public string Type { get; set; } = string.Empty;
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Content { get; set; }
	public string? ImageUrl { get; set; }
	public string? BackgroundColor { get; set; }
	public string? TextColor { get; set; }
	public int Order { get; set; }
	public bool IsActive { get; set; }
	public string? Settings { get; set; }
	public string? DataSource { get; set; }
	public int? DataCount { get; set; }
}

public class CreatePageDto
{
	public string Title { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? MetaKeywords { get; set; }
	public bool IsPublished { get; set; } = true;
	public bool IsHomePage { get; set; }
	public bool ShowInMenu { get; set; }
	public string? Template { get; set; }
	public Guid? ParentId { get; set; }
}

public class UpdatePageDto : CreatePageDto
{
	public Guid Id { get; set; }
}

public class CreatePageBlockDto
{
	public string Type { get; set; } = string.Empty;
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Content { get; set; }
	public string? ImageUrl { get; set; }
	public string? BackgroundColor { get; set; }
	public string? TextColor { get; set; }
	public bool IsActive { get; set; } = true;
	public string? Settings { get; set; }
	public string? DataSource { get; set; }
	public int? DataCount { get; set; }
}

public class UpdatePageBlockDto : CreatePageBlockDto
{
	public Guid Id { get; set; }
	public int Order { get; set; }
}

public class MenuDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Location { get; set; }
	public List<MenuItemDto> Items { get; set; } = new();
}

public class MenuItemDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
	public string? Target { get; set; }
	public string? Icon { get; set; }
	public int Order { get; set; }
	public Guid? ParentId { get; set; }
	public List<MenuItemDto> Children { get; set; } = new();
}