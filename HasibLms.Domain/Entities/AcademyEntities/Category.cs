using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.AcademyEntities;

public class Category : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid? ParentId { get; set; }
	[ForeignKey(nameof(ParentId))]
	public Category? Parent { get; set; }

	public ICollection<Category> Children { get; set; } = new List<Category>();
	public ICollection<Course> Courses { get; set; } = new List<Course>();
}