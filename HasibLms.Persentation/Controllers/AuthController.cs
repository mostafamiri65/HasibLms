using HasibLms.Application.Services;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

public class AuthController : Controller
{
	private readonly IAuthService _authService;
	private readonly ISiteService _siteService;
	private readonly ILogger<AuthController> _logger;
	private readonly IInstructorRequestService _instructorRequestService;

	public AuthController(
		IAuthService authService,
		ISiteService siteService,
		ILogger<AuthController> logger, IInstructorRequestService instructorRequestService)
	{
		_authService = authService;
		_siteService = siteService;
		_logger = logger;
		_instructorRequestService = instructorRequestService;
	}

	[HttpGet]
	public async Task<IActionResult> Login(string? returnUrl = null)
	{
		if (User.Identity?.IsAuthenticated == true)
			return RedirectToAction("Index", "Home");

		ViewBag.ReturnUrl = returnUrl;
		ViewBag.Title = "ورود به حساب کاربری";

		var settings = await _siteService.GetSettingsAsync();
		ViewBag.SiteName = settings?.InstituteShortName;

		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
	{
		if (ModelState.IsValid)
		{
			var result = await _authService.LoginAsync(model);
			if (result.Succeeded)
			{
				_logger.LogInformation("User {Email} logged in", model.Email);

				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					return Redirect(returnUrl);

				return RedirectToAction("Index", "Home");
			}

			ModelState.AddModelError(string.Empty, result.Message ?? "خطا در ورود");
		}

		ViewBag.ReturnUrl = returnUrl;
		return View(model);
	}

	[HttpGet]
	public async Task<IActionResult> Register()
	{
		if (User.Identity?.IsAuthenticated == true)
			return RedirectToAction("Index", "Home");

		ViewBag.Title = "ثبت نام در سایت";

		var settings = await _siteService.GetSettingsAsync();
		if (settings?.AllowRegistration == false)
		{
			TempData["Error"] = "ثبت نام موقتاً غیرفعال شده است";
			return RedirectToAction("Login");
		}

		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Register(RegisterDto model)
	{
		if (ModelState.IsValid)
		{
			var result = await _authService.RegisterAsync(model);
			if (result.Succeeded)
			{
				_logger.LogInformation("New user registered: {Email}", model.Email);
				TempData["Success"] = result.Message;
				return RedirectToAction("Index", "Home");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		return View(model);
	}

	[Authorize]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Logout()
	{
		await _authService.LogoutAsync();
		_logger.LogInformation("User logged out");
		return RedirectToAction("Index", "Home");
	}

	[Authorize]
	[HttpGet]
	public async Task<IActionResult> Profile()
	{
		ViewBag.Title = "پروفایل کاربری";

		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
		var profile = await _authService.GetUserProfileAsync(userId);

		if (profile == null)
			return NotFound();

		return View(profile);
	}

	[Authorize]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> UpdateProfile(UpdateProfileDto model)
	{
		if (ModelState.IsValid)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _authService.UpdateProfileAsync(model, userId);

			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Profile");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		return View("Profile");
	}

	[Authorize]
	[HttpGet]
	public IActionResult ChangePassword()
	{
		ViewBag.Title = "تغییر رمز عبور";
		return View();
	}

	[Authorize]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
	{
		if (ModelState.IsValid)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _authService.ChangePasswordAsync(model, userId);

			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Profile");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		return View(model);
	}

	[HttpGet]
	public IActionResult ForgotPassword()
	{
		ViewBag.Title = "فراموشی رمز عبور";
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
	{
		if (ModelState.IsValid)
		{
			var result = await _authService.ForgotPasswordAsync(model);
			TempData["Success"] = result.Message;
			return RedirectToAction("Login");
		}

		return View(model);
	}

	[HttpGet]
	public IActionResult ResetPassword(string email, string token)
	{
		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
			return BadRequest();

		ViewBag.Title = "بازنشانی رمز عبور";
		var model = new ResetPasswordDto { Email = email, Token = token };
		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
	{
		if (ModelState.IsValid)
		{
			var result = await _authService.ResetPasswordAsync(model);

			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Login");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		return View(model);
	}

	[Authorize]
	[HttpGet]
	public IActionResult AccessDenied()
	{
		ViewBag.Title = "دسترسی غیرمجاز";
		return View();
	}

	[Authorize]
	[HttpGet]
	public async Task<IActionResult> ApplyForInstructor(CancellationToken cancellationToken)
	{
		ViewBag.Title = "درخواست مدرس شدن";

		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
		var canApply = await _instructorRequestService.CanApplyForInstructorAsync(userId, cancellationToken);

		if (!canApply)
		{
			TempData["Error"] = "شما قبلاً درخواست مدرس شدن داده‌اید یا مدرس هستید.";
			return RedirectToAction("Profile");
		}

		return View();
	}

	[Authorize]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ApplyForInstructor(ApplyForInstructorDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _instructorRequestService.ApplyForInstructorAsync(model, userId, cancellationToken);

			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Profile");
			}

			ModelState.AddModelError(string.Empty, result.Message ?? "خطا در ثبت درخواست");
		}

		return View(model);
	}
}