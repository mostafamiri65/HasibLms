using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Entities.InvestigativeEntities;
using Microsoft.AspNetCore.Identity;

namespace HasibLms.Domain.Entities;

public class User : IdentityUser<Guid>
{
	public string FullName { get; set; } = string.Empty;
	public string? Bio { get; set; }
	public string? AvatarUrl { get; set; }
	public string? LinkedInUrl { get; set; }
	public DateTime CreatedDate { get; set; } = DateTime.Now;

	// Navigation
	public virtual ICollection<Course> TaughtCourses { get; set; } = new List<Course>();
	public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
	public virtual ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
	public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}