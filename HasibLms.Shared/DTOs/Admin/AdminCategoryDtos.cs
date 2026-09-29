using System.ComponentModel.DataAnnotations;

namespace HasibLms.Shared.DTOs.Admin;

public class CategoryListAdminDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid? ParentId { get; set; }
	public string? ParentName { get; set; }
	public int CourseCount { get; set; }
	public int ChildrenCount { get; set; }
	public DateTime CreatedDate { get; set; }
}

public class CreateCategoryDto
{
	[Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
	[MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
	[Display(Name = "نام دسته‌بندی")]
	public string Name { get; set; } = string.Empty;

	[Display(Name = "Slug")]
	[MaxLength(300, ErrorMessage = "حداکثر 300 کاراکتر")]
	[RegularExpression(@"^[a-z0-9\-]*$", ErrorMessage = "Slug فقط می‌تواند شامل حروف کوچک انگلیسی، اعداد و خط تیره باشد")]
	public string? Slug { get; set; }

	[Display(Name = "توضیحات")]
	[MaxLength(1000, ErrorMessage = "حداکثر 1000 کاراکتر")]
	public string? Description { get; set; }

	[Display(Name = "دسته‌بندی والد")]
	public Guid? ParentId { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto
{
	public Guid Id { get; set; }
}