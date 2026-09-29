using System.ComponentModel.DataAnnotations;

namespace HasibLms.Shared.DTOs.Settings;

public class MenuItemDto
{
	public Guid Id { get; set; }

	[Required(ErrorMessage = "عنوان منو الزامی است")]
	[MaxLength(100, ErrorMessage = "حداکثر 100 کاراکتر")]
	public string Title { get; set; } = string.Empty;

	[MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
	public string? Url { get; set; }

	public string? Icon { get; set; }
	public int Order { get; set; }
	public bool IsActive { get; set; } = true;
	public string? Target { get; set; }
	public string? CssClass { get; set; }
	public Guid? ParentId { get; set; }
	public string Location { get; set; } = "header";
	public List<MenuItemDto> Children { get; set; } = new();
}

public class CreateMenuItemDto
{
	[Required(ErrorMessage = "عنوان منو الزامی است")]
	[MaxLength(100, ErrorMessage = "حداکثر 100 کاراکتر")]
	public string Title { get; set; } = string.Empty;

	public string? Url { get; set; }
	public string? Icon { get; set; }
	public string? Target { get; set; }
	public string? CssClass { get; set; }
	public Guid? ParentId { get; set; }
	public string Location { get; set; } = "header";
}

public class UpdateMenuItemDto : CreateMenuItemDto
{
	public Guid Id { get; set; }
	public bool IsActive { get; set; } = true;
}

public class MenuSettingsDto
{
	public List<MenuItemDto> HeaderMenuItems { get; set; } = new();
	public List<MenuItemDto> FooterMenuItems { get; set; } = new();
}