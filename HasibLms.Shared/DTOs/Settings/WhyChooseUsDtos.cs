using System.ComponentModel.DataAnnotations;

namespace HasibLms.Shared.DTOs.Settings;

public class WhyChooseUsItemDto
{
	public Guid Id { get; set; }

	[Required(ErrorMessage = "عنوان الزامی است")]
	[MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
	public string Title { get; set; } = string.Empty;

	[Required(ErrorMessage = "توضیحات الزامی است")]
	[MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
	public string Description { get; set; } = string.Empty;

	public string? Icon { get; set; }
	public int Order { get; set; }
	public bool IsActive { get; set; } = true;
	public bool ShowOnHomePage { get; set; } = true;
	public string? BackgroundColor { get; set; }
	public string? IconColor { get; set; }
}

public class CreateWhyChooseUsItemDto
{
	[Required(ErrorMessage = "عنوان الزامی است")]
	[MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
	public string Title { get; set; } = string.Empty;

	[Required(ErrorMessage = "توضیحات الزامی است")]
	[MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
	public string Description { get; set; } = string.Empty;

	public string? Icon { get; set; }
	public bool IsActive { get; set; } = true;
	public bool ShowOnHomePage { get; set; } = true;
	public string? BackgroundColor { get; set; }
	public string? IconColor { get; set; }
}

public class UpdateWhyChooseUsItemDto : CreateWhyChooseUsItemDto
{
	public Guid Id { get; set; }
}

public class WhyChooseUsSettingsDto
{
	public bool ShowSection { get; set; } = true;
	public string? SectionTitle { get; set; } = "چرا باید ما را انتخاب کنید؟";
	public string? SectionSubtitle { get; set; }
	public List<WhyChooseUsItemDto> Items { get; set; } = new();
}