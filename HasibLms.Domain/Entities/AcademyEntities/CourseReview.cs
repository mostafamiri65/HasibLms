using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.AcademyEntities;

public class CourseReview : BaseEntity
{
	public int Rating { get; set; } // ۱ تا ۵
	public string? Comment { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public bool IsApproved { get; set; } = false;

	public Guid CourseId { get; set; }
	[ForeignKey(nameof(CourseId))]
	public Course Course { get; set; } = null!;

	public Guid StudentId { get; set; }
	[ForeignKey(nameof(StudentId))]
	public User Student { get; set; } = null!;
}