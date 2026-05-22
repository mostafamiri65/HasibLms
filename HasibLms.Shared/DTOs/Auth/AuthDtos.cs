using System.ComponentModel.DataAnnotations;

namespace HasibLms.Shared.DTOs.Auth;

public class RegisterDto
{
	[Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
	[Display(Name = "نام و نام خانوادگی")]
	[MinLength(3, ErrorMessage = "حداقل 3 کاراکتر")]
	[MaxLength(100, ErrorMessage = "حداکثر 100 کاراکتر")]
	public string FullName { get; set; } = string.Empty;

	[Required(ErrorMessage = "ایمیل الزامی است")]
	[EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
	[Display(Name = "ایمیل")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "رمز عبور الزامی است")]
	[MinLength(6, ErrorMessage = "حداقل 6 کاراکتر")]
	[Display(Name = "رمز عبور")]
	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

	[Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
	[Compare("Password", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
	[Display(Name = "تکرار رمز عبور")]
	[DataType(DataType.Password)]
	public string ConfirmPassword { get; set; } = string.Empty;

	[Display(Name = "شماره موبایل")]
	[Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
	public string? PhoneNumber { get; set; }
}

public class LoginDto
{
	[Required(ErrorMessage = "ایمیل الزامی است")]
	[EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
	[Display(Name = "ایمیل")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "رمز عبور الزامی است")]
	[Display(Name = "رمز عبور")]
	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

	[Display(Name = "مرا به خاطر بسپار")]
	public bool RememberMe { get; set; }
}

public class ChangePasswordDto
{
	[Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
	[Display(Name = "رمز عبور فعلی")]
	[DataType(DataType.Password)]
	public string CurrentPassword { get; set; } = string.Empty;

	[Required(ErrorMessage = "رمز عبور جدید الزامی است")]
	[MinLength(6, ErrorMessage = "حداقل 6 کاراکتر")]
	[Display(Name = "رمز عبور جدید")]
	[DataType(DataType.Password)]
	public string NewPassword { get; set; } = string.Empty;

	[Required(ErrorMessage = "تکرار رمز عبور جدید الزامی است")]
	[Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تکرار آن مطابقت ندارند")]
	[Display(Name = "تکرار رمز عبور جدید")]
	[DataType(DataType.Password)]
	public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
	[Required(ErrorMessage = "ایمیل الزامی است")]
	[EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
	[Display(Name = "ایمیل")]
	public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	public string Token { get; set; } = string.Empty;

	[Required(ErrorMessage = "رمز عبور جدید الزامی است")]
	[MinLength(6, ErrorMessage = "حداقل 6 کاراکتر")]
	[Display(Name = "رمز عبور جدید")]
	[DataType(DataType.Password)]
	public string NewPassword { get; set; } = string.Empty;

	[Required(ErrorMessage = "تکرار رمز عبور جدید الزامی است")]
	[Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تکرار آن مطابقت ندارند")]
	[Display(Name = "تکرار رمز عبور جدید")]
	[DataType(DataType.Password)]
	public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
	[Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
	[Display(Name = "نام و نام خانوادگی")]
	public string FullName { get; set; } = string.Empty;

	[Display(Name = "بیوگرافی")]
	[MaxLength(500, ErrorMessage = "حداکثر 500 کاراکتر")]
	public string? Bio { get; set; }

	[Display(Name = "شماره موبایل")]
	[Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
	public string? PhoneNumber { get; set; }

	[Display(Name = "لینک LinkedIn")]
	[Url(ErrorMessage = "لینک معتبر نیست")]
	public string? LinkedInUrl { get; set; }

	public string? AvatarUrl { get; set; }
}

public class UserProfileDto
{
	public Guid Id { get; set; }
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? Bio { get; set; }
	public string? AvatarUrl { get; set; }
	public string? LinkedInUrl { get; set; }
	public string? PhoneNumber { get; set; }
	public List<string> Roles { get; set; } = new();
	public int EnrolledCoursesCount { get; set; }
	public int TaughtCoursesCount { get; set; }
	public int ArticlesCount { get; set; }
	public int ReviewsCount { get; set; }
	public DateTime MemberSince { get; set; }
}

public class AuthResultDto
{
	public bool Succeeded { get; set; }
	public string? Message { get; set; }
	public List<string> Errors { get; set; } = new();
	public UserProfileDto? User { get; set; }
	public object? Data { get; set; }
}