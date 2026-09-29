using HasibLms.Shared.DTOs.Instructor;
using System.ComponentModel.DataAnnotations;

namespace HasibLms.Shared.DTOs.Admin;

public class CreateCourseByAdminDto : CreateCourseDto
{
	[Required(ErrorMessage = "انتخاب مدرس الزامی است")]
	[Display(Name = "مدرس دوره")]
	public Guid InstructorId { get; set; }

	[Display(Name = "وضعیت انتشار")]
	public bool IsPublished { get; set; } = false;

	[Display(Name = "دوره منتخب")]
	public bool IsFeatured { get; set; } = false;
}

public class UpdateCourseByAdminDto : UpdateCourseDto
{
	[Required(ErrorMessage = "انتخاب مدرس الزامی است")]
	[Display(Name = "مدرس دوره")]
	public Guid InstructorId { get; set; }

	[Display(Name = "وضعیت انتشار")]
	public bool IsPublished { get; set; }

	[Display(Name = "دوره منتخب")]
	public bool IsFeatured { get; set; }
}

public class InstructorSelectDto
{
	public Guid Id { get; set; }
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string DisplayName => $"{FullName} ({Email})";
	public bool IsInstructor { get; set; }
}