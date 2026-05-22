using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.AcademyEntities;

public class Enrollment : BaseEntity
{
	public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

	public Guid StudentId { get; set; }
	[ForeignKey(nameof(StudentId))]
	public User Student { get; set; } = null!;

	public Guid CourseId { get; set; }
	[ForeignKey(nameof(CourseId))]
	public Course Course { get; set; } = null!;
}