using HasibLms.Shared.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace HasibLms.Shared.DTOs.Instructor;

public class CreateCourseDto
{
	[Required(ErrorMessage = "عنوان دوره الزامی است")]
	[Display(Name = "عنوان دوره")]
	[MinLength(5, ErrorMessage = "حداقل 5 کاراکتر")]
	[MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
	public string Title { get; set; } = string.Empty;

	[Display(Name = "توضیحات کوتاه")]
	[MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
	public string ShortDescription { get; set; } = string.Empty;

	[Required(ErrorMessage = "توضیحات دوره الزامی است")]
	[Display(Name = "توضیحات کامل")]
	public string Description { get; set; } = string.Empty;

	[Display(Name = "قیمت (تومان)")]
	[Range(0, 100000000, ErrorMessage = "قیمت نامعتبر است")]
	public decimal Price { get; set; }

	[Display(Name = "قیمت با تخفیف")]
	[Range(0, 100000000, ErrorMessage = "قیمت نامعتبر است")]
	public decimal? DiscountPrice { get; set; }

	[Display(Name = "تصویر دوره")]
	public string? ImageUrl { get; set; }

	[Display(Name = "ویدیو معرفی")]
	public string? IntroVideoUrl { get; set; }

	[Display(Name = "نوع دوره")]
	public CourseType CourseType { get; set; } = CourseType.Online;

	[Display(Name = "تاریخ شروع")]
	public DateTime StartDate { get; set; } = DateTime.Now.AddDays(7);

	[Display(Name = "تاریخ پایان")]
	public DateTime? EndDate { get; set; }

	[Display(Name = "مدت دوره (ساعت)")]
	[Range(1, 500, ErrorMessage = "مدت دوره باید بین 1 تا 500 ساعت باشد")]
	public int DurationHours { get; set; }

	[Display(Name = "ظرفیت")]
	[Range(1, 1000, ErrorMessage = "ظرفیت باید بین 1 تا 1000 باشد")]
	public int Capacity { get; set; } = 50;

	[Display(Name = "دسته‌بندی")]
	public Guid CategoryId { get; set; }

	[Display(Name = "عنوان SEO")]
	[MaxLength(70, ErrorMessage = "حداکثر 70 کاراکتر")]
	public string? MetaTitle { get; set; }

	[Display(Name = "توضیحات SEO")]
	[MaxLength(160, ErrorMessage = "حداکثر 160 کاراکتر")]
	public string? MetaDescription { get; set; }
}

public class UpdateCourseDto : CreateCourseDto
{
	public Guid Id { get; set; }
}

public class CourseEditDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string ShortDescription { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public decimal? DiscountPrice { get; set; }
	public string? ImageUrl { get; set; }
	public string? IntroVideoUrl { get; set; }
	public CourseType CourseType { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public int DurationHours { get; set; }
	public int Capacity { get; set; }
	public Guid CategoryId { get; set; }
	public bool IsPublished { get; set; }
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
}
public class AddSyllabusDto
{
    [Required(ErrorMessage = "شناسه دوره الزامی است")]
    public Guid CourseId { get; set; }

    [Required(ErrorMessage = "عنوان سرفصل الزامی است")]
    [MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
    public string? Description { get; set; }
}

public class UpdateSyllabusDto
{
    [Required(ErrorMessage = "شناسه سرفصل الزامی است")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "عنوان سرفصل الزامی است")]
    [MaxLength(200, ErrorMessage = "حداکثر 200 کاراکتر")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
    public string? Description { get; set; }
}
public class SyllabusDetailDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public int Order { get; set; }
	public Guid CourseId { get; set; }
	public string CourseTitle { get; set; } = string.Empty;
	public List<LessonDetailDto> Lessons { get; set; } = new();
}

public class LessonDetailDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Content { get; set; }
	public string? VideoUrl { get; set; }
	public int DurationMinutes { get; set; }
	public bool IsPreview { get; set; }
	public int Order { get; set; }
}

public class AddLessonDto
{
	public Guid SyllabusId { get; set; }

	[Required(ErrorMessage = "عنوان جلسه الزامی است")]
	public string Title { get; set; } = string.Empty;

	public string? Content { get; set; }
	public string? VideoUrl { get; set; }
	public int DurationMinutes { get; set; }
	public bool IsPreview { get; set; }
}

public class UpdateLessonDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Content { get; set; }
	public string? VideoUrl { get; set; }
	public int DurationMinutes { get; set; }
	public bool IsPreview { get; set; }
}

public class CourseStatisticsDto
{
	public Guid CourseId { get; set; }
	public string CourseTitle { get; set; } = string.Empty;
	public int TotalEnrollments { get; set; }
	public int TotalStudents { get; set; }
	public decimal TotalRevenue { get; set; }
	public double AverageRating { get; set; }
	public int TotalReviews { get; set; }
	public int Capacity { get; set; }
	public int RemainingCapacity { get; set; }
	public List<MonthlyEnrollmentDto> MonthlyEnrollments { get; set; } = new();
}

public class MonthlyEnrollmentDto
{
	public string Month { get; set; } = string.Empty;
	public int Count { get; set; }
}