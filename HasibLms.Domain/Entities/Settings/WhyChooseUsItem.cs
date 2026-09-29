namespace HasibLms.Domain.Entities.Settings;

public class WhyChooseUsItem : BaseEntity
{
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string? Icon { get; set; } // کلاس آیکون FontAwesome
	public int Order { get; set; }
	public bool IsActive { get; set; } = true;
	public bool ShowOnHomePage { get; set; } = true;
	public string? BackgroundColor { get; set; }
	public string? IconColor { get; set; }
}