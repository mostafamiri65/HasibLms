using System.ComponentModel.DataAnnotations;
using HasibLms.Shared.Enumerations;

namespace HasibLms.Shared.DTOs.Instructor;

public class ApplyForInstructorDto
{
	[Display(Name = "نام و نام خانوادگی")]
	[Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
	public string FullName { get; set; } = string.Empty;

	[Display(Name = "ایمیل")]
	[Required(ErrorMessage = "ایمیل الزامی است")]
	[EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
	public string Email { get; set; } = string.Empty;

	[Display(Name = "شماره موبایل")]
	[Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
	public string? PhoneNumber { get; set; }

	[Display(Name = "مدرک تحصیلی")]
	public string? Degree { get; set; }

	[Display(Name = "دانشگاه")]
	public string? University { get; set; }

	[Display(Name = "سال‌های تجربه")]
	[Range(0, 50, ErrorMessage = "سال‌های تجربه باید بین 0 تا 50 باشد")]
	public int? YearsOfExperience { get; set; }

	[Display(Name = "حوزه تخصصی")]
	public string? Expertise { get; set; }

	[Display(Name = "بیوگرافی")]
	[MaxLength(1000, ErrorMessage = "حداکثر 1000 کاراکتر")]
	public string? Bio { get; set; }

	[Display(Name = "سابقه تدریس")]
	public string? TeachingExperience { get; set; }

	[Display(Name = "لینک نمونه کار")]
	[Url(ErrorMessage = "لینک معتبر نیست")]
	public string? SampleWorkUrl { get; set; }

	[Display(Name = "آدرس فایل رزومه")]
	public string? ResumeUrl { get; set; }

	[Display(Name = "آدرس فایل مدرک")]
	public string? CertificateUrl { get; set; }
}

public class InstructorRequestListDto
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? PhoneNumber { get; set; }
	public string? Degree { get; set; }
	public string? University { get; set; }
	public int? YearsOfExperience { get; set; }
	public string? Expertise { get; set; }
	public string? Bio { get; set; }
	public string? ResumeUrl { get; set; }
	public string? CertificateUrl { get; set; }
	public RequestStatus Status { get; set; }
	public string StatusText => Status switch
	{
		RequestStatus.Pending => "در انتظار بررسی",
		RequestStatus.Approved => "تأیید شده",
		RequestStatus.Rejected => "رد شده",
		_ => "نامشخص"
	};
	public string StatusColor => Status switch
	{
		RequestStatus.Pending => "yellow",
		RequestStatus.Approved => "green",
		RequestStatus.Rejected => "red",
		_ => "gray"
	};
	public DateTime RequestedAt { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public string? RejectReason { get; set; }
}

public class ReviewInstructorRequestDto
{
	public Guid RequestId { get; set; }
	public bool IsApproved { get; set; }
	public string? RejectReason { get; set; }
}