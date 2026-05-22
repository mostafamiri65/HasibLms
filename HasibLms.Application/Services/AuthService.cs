using HasibLms.Domain.Entities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.Constants;
using HasibLms.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class AuthService : IAuthService
{
	private readonly UserManager<User> _userManager;
	private readonly SignInManager<User> _signInManager;
	private readonly RoleManager<IdentityRole<Guid>> _roleManager;
	private readonly ILogger<AuthService> _logger;
	private readonly IEnrollmentService _enrollmentService;
	private readonly IArticleRepository _articleRepository;

	public AuthService(
		UserManager<User> userManager,
		SignInManager<User> signInManager,
		RoleManager<IdentityRole<Guid>> roleManager,
		ILogger<AuthService> logger,
		IEnrollmentService enrollmentService,
		IArticleRepository articleRepository)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_roleManager = roleManager;
		_logger = logger;
		_enrollmentService = enrollmentService;
		_articleRepository = articleRepository;
	}

	public async Task<AuthResultDto> RegisterAsync(RegisterDto model, CancellationToken cancellationToken = default)
	{
		try
		{
			// Check if user exists
			var existingUser = await _userManager.FindByEmailAsync(model.Email);
			if (existingUser != null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "این ایمیل قبلاً ثبت نام کرده است"
				};
			}

			// Create user
			var user = new User
			{
				UserName = model.Email,
				Email = model.Email,
				FullName = model.FullName,
				PhoneNumber = model.PhoneNumber,
				CreatedDate = DateTime.Now
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Errors = result.Errors.Select(e => e.Description).ToList()
				};
			}

			// Assign Student role by default
			if (!await _roleManager.RoleExistsAsync(UserRoles.Student))
				await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRoles.Student));

			await _userManager.AddToRoleAsync(user, UserRoles.Student);

			// Sign in
			await _signInManager.SignInAsync(user, isPersistent: false);

			_logger.LogInformation("New user registered: {Email}", model.Email);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "ثبت نام با موفقیت انجام شد",
				User = await MapToUserProfileAsync(user)
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during registration for {Email}", model.Email);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ثبت نام. لطفاً مجدداً تلاش کنید"
			};
		}
	}

	public async Task<AuthResultDto> LoginAsync(LoginDto model, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "ایمیل یا رمز عبور اشتباه است"
				};
			}

			var result = await _signInManager.PasswordSignInAsync(
				user, model.Password, model.RememberMe, lockoutOnFailure: false);

			if (!result.Succeeded)
			{
				if (result.IsLockedOut)
				{
					return new AuthResultDto
					{
						Succeeded = false,
						Message = "حساب کاربری شما به دلیل تلاش‌های مکرر قفل شده است"
					};
				}

				return new AuthResultDto
				{
					Succeeded = false,
					Message = "ایمیل یا رمز عبور اشتباه است"
				};
			}

			_logger.LogInformation("User logged in: {Email}", model.Email);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "ورود با موفقیت انجام شد",
				User = await MapToUserProfileAsync(user)
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during login for {Email}", model.Email);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ورود. لطفاً مجدداً تلاش کنید"
			};
		}
	}

	public async Task LogoutAsync()
	{
		await _signInManager.SignOutAsync();
		_logger.LogInformation("User logged out");
	}

	public async Task<AuthResultDto> ChangePasswordAsync(ChangePasswordDto model, Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId.ToString());
		if (user == null)
		{
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "کاربر یافت نشد"
			};
		}

		var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

		if (!result.Succeeded)
		{
			return new AuthResultDto
			{
				Succeeded = false,
				Errors = result.Errors.Select(e => e.Description).ToList()
			};
		}

		await _signInManager.RefreshSignInAsync(user);
		_logger.LogInformation("Password changed for user: {Email}", user.Email);

		return new AuthResultDto
		{
			Succeeded = true,
			Message = "رمز عبور با موفقیت تغییر کرد"
		};
	}

	public async Task<AuthResultDto> ForgotPasswordAsync(ForgotPasswordDto model, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByEmailAsync(model.Email);
		if (user == null)
		{
			// Don't reveal that the user doesn't exist
			return new AuthResultDto
			{
				Succeeded = true,
				Message = "در صورت وجود ایمیل، لینک بازیابی ارسال خواهد شد"
			};
		}

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);

		// TODO: Send email with token
		_logger.LogInformation("Password reset token for {Email}: {Token}", model.Email, token);

		return new AuthResultDto
		{
			Succeeded = true,
			Message = "لینک بازیابی رمز عبور به ایمیل شما ارسال شد"
		};
	}

	public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto model, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByEmailAsync(model.Email);
		if (user == null)
		{
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "کاربر یافت نشد"
			};
		}

		var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

		if (!result.Succeeded)
		{
			return new AuthResultDto
			{
				Succeeded = false,
				Errors = result.Errors.Select(e => e.Description).ToList()
			};
		}

		_logger.LogInformation("Password reset for user: {Email}", model.Email);

		return new AuthResultDto
		{
			Succeeded = true,
			Message = "رمز عبور با موفقیت بازنشانی شد"
		};
	}

	public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users
			.Include(u => u.Enrollments)
			.Include(u => u.TaughtCourses)
			.Include(u => u.Articles)
			.Include(u => u.Reviews)
			.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user == null) return null;

		return await MapToUserProfileAsync(user);
	}

	public async Task<AuthResultDto> UpdateProfileAsync(UpdateProfileDto model, Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId.ToString());
		if (user == null)
		{
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "کاربر یافت نشد"
			};
		}

		user.FullName = model.FullName;
		user.Bio = model.Bio;
		user.PhoneNumber = model.PhoneNumber;
		user.LinkedInUrl = model.LinkedInUrl;
		if (!string.IsNullOrEmpty(model.AvatarUrl))
			user.AvatarUrl = model.AvatarUrl;


		var result = await _userManager.UpdateAsync(user);

		if (!result.Succeeded)
		{
			return new AuthResultDto
			{
				Succeeded = false,
				Errors = result.Errors.Select(e => e.Description).ToList()
			};
		}

		_logger.LogInformation("Profile updated for user: {Email}", user.Email);

		return new AuthResultDto
		{
			Succeeded = true,
			Message = "پروفایل با موفقیت بروزرسانی شد",
			User = await MapToUserProfileAsync(user)
		};
	}

	private async Task<UserProfileDto> MapToUserProfileAsync(User user)
	{
		var roles = await _userManager.GetRolesAsync(user);
		var enrolledCount = await _enrollmentService.GetUserEnrollmentCountAsync(user.Id, CancellationToken.None);

		return new UserProfileDto
		{
			Id = user.Id,
			FullName = user.FullName,
			Email = user.Email ?? string.Empty,
			Bio = user.Bio,
			AvatarUrl = user.AvatarUrl,
			LinkedInUrl = user.LinkedInUrl,
			PhoneNumber = user.PhoneNumber,
			Roles = roles.ToList(),
			EnrolledCoursesCount = enrolledCount,
			TaughtCoursesCount = user.TaughtCourses?.Count(c => !c.IsDeleted) ?? 0,
			ArticlesCount = user.Articles?.Count(a => !a.IsDeleted && a.IsPublished) ?? 0,
			ReviewsCount = user.Reviews?.Count(r => !r.IsDeleted) ?? 0,
			MemberSince = user.CreatedDate
		};
	}
}