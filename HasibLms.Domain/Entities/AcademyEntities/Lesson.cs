using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.AcademyEntities;

public class Lesson : BaseEntity
{
	public string Title { get; set; } = string.Empty;
	public string? Content { get; set; } // محتوای HTML (اختیاری)
	public string? VideoUrl { get; set; } // لینک ویدیو در CDN
	public int DurationMinutes { get; set; } // مدت زمان ویدیو
	public bool IsPreview { get; set; } // پیش‌نمایش رایگان
	public int Order { get; set; }

	public Guid SyllabusId { get; set; }
	[ForeignKey(nameof(SyllabusId))]
	public Syllabus Syllabus { get; set; } = null!;

	public Guid CourseId { get; set; }
	[ForeignKey(nameof(CourseId))]
	public Course Course { get; set; } = null!;
}