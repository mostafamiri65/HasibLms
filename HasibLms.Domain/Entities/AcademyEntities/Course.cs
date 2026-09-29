using HasibLms.Shared.Enumerations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HasibLms.Domain.Entities.AcademyEntities;

public class Course : BaseEntity
{
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty; // برای SEO: course/how-to-accounting
	public string ShortDescription { get; set; } = string.Empty; // توضیح کوتاه برای کارت دوره
	public string Description { get; set; } = string.Empty; // توضیح کامل (HTML)
	public decimal Price { get; set; }
	public decimal? DiscountPrice { get; set; } // قیمت تخفیف‌خورده
	public string? ImageUrl { get; set; }
	public string? IntroVideoUrl { get; set; }  // ویدیو معرفی (قبلاً بود)
	public string? FullVideoUrl { get; set; }   // ✅ جدید: ویدیوی اصلی دوره

	public CourseType CourseType { get; set; }
	public DateTime StartDate { get; set; } // برای Online/InPerson
	public DateTime? EndDate { get; set; }
	public int Capacity { get; set; } = 50; // ظرفیت
	public int DurationHours { get; set; } // مدت دوره به ساعت

	public bool IsPublished { get; set; }
	public bool IsFeatured { get; set; } // دوره منتخب


	// SEO
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? CanonicalUrl { get; set; }

	// Foreign Keys
	public Guid InstructorId { get; set; }
	[ForeignKey(nameof(InstructorId))]
	public User Instructor { get; set; } = null!;
	public Guid CategoryId { get; set; }
	[ForeignKey(nameof(CategoryId))]
	public Category Category { get; set; } = null!;

	// Navigation Properties
	public virtual ICollection<Syllabus> Syllabuses { get; set; } = new List<Syllabus>();
	public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
	public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
	public virtual ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();

	// متدهای Domain
	public void Publish() => IsPublished = true;
	public void Unpublish() => IsPublished = false;
	public void Feature() => IsFeatured = true;
	public void UnFeature() => IsFeatured = false;
	public int EnrolledCount => Enrollments?.Count ?? 0;
	public bool HasCapacity => EnrolledCount < Capacity;
}