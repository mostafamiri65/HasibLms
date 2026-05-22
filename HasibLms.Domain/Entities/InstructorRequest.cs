using System.ComponentModel.DataAnnotations.Schema;
using HasibLms.Shared.Enumerations;

namespace HasibLms.Domain.Entities;

public class InstructorRequest : BaseEntity
{
	public Guid UserId { get; set; }
	[ForeignKey(nameof(UserId))]
	public User User { get; set; } = null!;

	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? PhoneNumber { get; set; }

	// اطلاعات حرفه‌ای
	public string? Degree { get; set; } // مدرک تحصیلی
	public string? University { get; set; } // دانشگاه
	public int? YearsOfExperience { get; set; } // سال‌های تجربه
	public string? Expertise { get; set; } // تخصص

	// مدارک
	public string? ResumeUrl { get; set; } // آدرس فایل رزومه
	public string? CertificateUrl { get; set; } // آدرس فایل مدرک

	// معرفی
	public string? Bio { get; set; } // بیوگرافی
	public string? TeachingExperience { get; set; } // سابقه تدریس
	public string? SampleWorkUrl { get; set; } // نمونه کار

	// وضعیت
	public RequestStatus Status { get; set; } = RequestStatus.Pending;
	public string? RejectReason { get; set; }

	public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
	public DateTime? ReviewedAt { get; set; }
	public Guid? ReviewedBy { get; set; }
}