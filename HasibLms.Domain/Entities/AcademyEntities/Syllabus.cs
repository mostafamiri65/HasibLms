using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.AcademyEntities;

public class Syllabus : BaseEntity
{
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public int Order { get; set; }

	public Guid CourseId { get; set; }
	[ForeignKey(nameof(CourseId))]
	public Course Course { get; set; } = null!;

	public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}