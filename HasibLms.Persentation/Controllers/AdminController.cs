using HasibLms.Application.Services;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Instructor;
using HasibLms.Shared.DTOs.Settings;
using HasibLms.Shared.Enumerations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HasibLms.Persentation.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
	private readonly IAdminService _adminService;
	private readonly ICourseService _courseService;
	private readonly IReviewService _reviewService;
	private readonly ISiteService _siteService;
	private readonly ICategoryService _categoryService;

	private readonly IInstructorRequestService _instructorRequestService;
	private readonly ILogger<AdminController> _logger;

	public AdminController(
		IAdminService adminService,
		ICourseService courseService,
		IReviewService reviewService,
		ISiteService siteService,
		ILogger<AdminController> logger,
		IInstructorRequestService instructorRequestService,
		ICategoryService categoryService)
	{
		_adminService = adminService;
		_courseService = courseService;
		_reviewService = reviewService;
		_siteService = siteService;
		_logger = logger;
		_instructorRequestService = instructorRequestService;
		_categoryService = categoryService;
	}

	[HttpGet]
	public async Task<IActionResult> Index(CancellationToken cancellationToken)
	{
		ViewBag.Title = "داشبورد مدیریت";

		var stats = await _adminService.GetDashboardStatsAsync(cancellationToken);
		return View(stats);
	}

	#region User Management

	[HttpGet]
	public async Task<IActionResult> Users(int page = 1, string? role = null, CancellationToken cancellationToken = default)
	{
		ViewBag.Title = "مدیریت کاربران";

		var users = await _adminService.GetUsersAsync(page, 20, role, cancellationToken);
		ViewBag.CurrentRole = role;

		return View(users);
	}

	[HttpPost]
	public async Task<IActionResult> AssignRole(Guid userId, string role, CancellationToken cancellationToken)
	{
		var result = await _adminService.AssignRoleAsync(userId, role, cancellationToken);

		if (result.Succeeded)
			TempData["Success"] = result.Message;
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Users");
	}

	[HttpPost]
	public async Task<IActionResult> RemoveRole(Guid userId, string role, CancellationToken cancellationToken)
	{
		var result = await _adminService.RemoveRoleAsync(userId, role, cancellationToken);

		if (result.Succeeded)
			TempData["Success"] = result.Message;
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Users");
	}

	[HttpPost]
	public async Task<IActionResult> ToggleUserStatus(Guid userId, CancellationToken cancellationToken)
	{
		var result = await _adminService.ToggleUserStatusAsync(userId, cancellationToken);

		if (result.Succeeded)
			TempData["Success"] = result.Message;
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Users");
	}

	#endregion

	#region Course Management

	[HttpGet]
	public async Task<IActionResult> Courses(int page = 1, string? status = null, CancellationToken cancellationToken = default)
	{
		ViewBag.Title = "مدیریت دوره‌ها";

		var courses = await _adminService.GetCoursesForAdminAsync(page, 20, status, cancellationToken);
		ViewBag.StatusFilter = status;

		return View(courses);
	}

	[HttpPost]
	public async Task<IActionResult> ApproveCourse(Guid id, CancellationToken cancellationToken)
	{
		var result = await _adminService.ApproveCourseAsync(id, cancellationToken);

		if (result.Succeeded)
		{
			await _courseService.ClearCourseCacheAsync(id);
			TempData["Success"] = result.Message;
		}
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Courses");
	}
	[HttpPost]
	public async Task<IActionResult> UnPublishCourse(Guid id, CancellationToken cancellationToken)
	{
		var result = await _adminService.UnPublishCourse(id, cancellationToken);

		if (result.Succeeded)
		{
			await _courseService.ClearCourseCacheAsync(id);
			TempData["Success"] = result.Message;
		}
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Courses");
	}

	[HttpPost]
	public async Task<IActionResult> FeatureCourse(Guid id, bool isFeatured, CancellationToken cancellationToken)
	{
		var result = await _adminService.FeatureCourseAsync(id, isFeatured, cancellationToken);

		if (result.Succeeded)
		{
			await _courseService.ClearCourseCacheAsync(id);
			TempData["Success"] = result.Message;
		}
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Courses");
	}

	[HttpPost]
	public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken cancellationToken)
	{
		var result = await _adminService.DeleteCourseAsync(id, cancellationToken);

		if (result.Succeeded)
		{
			await _courseService.ClearCourseCacheAsync(id);
			TempData["Success"] = result.Message;
		}
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Courses");
	}

	#endregion

	#region Review Management

	[HttpGet]
	public async Task<IActionResult> Reviews(int page = 1, bool? pending = null, CancellationToken cancellationToken = default)
	{
		ViewBag.Title = "مدیریت نظرات";

		var reviews = await _adminService.GetReviewsForAdminAsync(page, 20, pending, cancellationToken);
		ViewBag.ShowPendingOnly = pending == true;

		return View(reviews);
	}

	[HttpPost]
	public async Task<IActionResult> ApproveReview(Guid id, CancellationToken cancellationToken)
	{
		var result = await _adminService.ApproveReviewAsync(id, cancellationToken);

		if (result.Succeeded)
		{
			await _reviewService.ClearReviewCacheAsync(id);
			TempData["Success"] = result.Message;
		}
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Reviews");
	}

	[HttpPost]
	public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
	{
		var result = await _adminService.DeleteReviewAsync(id, cancellationToken);

		if (result.Succeeded)
		{
			await _reviewService.ClearReviewCacheAsync(id);
			TempData["Success"] = result.Message;
		}
		else
			TempData["Error"] = result.Message;

		return RedirectToAction("Reviews");
	}

	#endregion

	#region Settings


	[HttpGet]
	public async Task<IActionResult> Settings(CancellationToken cancellationToken)
	{
		ViewBag.Title = "تنظیمات سایت";

		var settings = await _siteService.GetSettingsAsync(cancellationToken);
		return View(settings);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Settings(SiteSettingsDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			UpdateSiteSettingsDto dto = new UpdateSiteSettingsDto()
			{
				AboutText = model.AboutText,
				Address = model.Address,
				AllowRegistration = model.AllowRegistration,
				CopyrightText = model.CopyrightText,
				CurrencySymbol = model.CurrencySymbol,
				Email = model.Email,
				FaviconUrl = model.FaviconUrl,
				Fax = model.Fax,
				FooterText = model.FooterText,
				GoogleMapEmbed = model.GoogleMapEmbed,
				Instagram = model.Instagram,
				InstituteName = model.InstituteName,
				InstituteShortName = model.InstituteShortName,
				LinkedIn = model.LinkedIn,
				LogoUrl = model.LogoUrl,
				LogoWhiteUrl = model.LogoWhiteUrl,
				MapLatitude = model.MapLatitude,
				MapLongitude = model.MapLongitude,
				MetaDescription = model.MetaDescription,
				MetaKeywords = model.MetaKeywords,
				MetaTitle = model.MetaTitle,
				Mobile = model.Mobile,
				Phone = model.Phone,
				PostalCode = model.PostalCode,
				PrimaryColor = model.PrimaryColor,
				SecondaryColor = model.SecondaryColor,
				Slogan = model.Slogan,
				Telegram = model.Telegram,
				WhatsApp = model.WhatsApp,
				YouTube = model.YouTube,
				//howFeaturedCourses = model.ShowFeaturedCourses,
				ShowLatestCourses = model.ShowLatestCourses,
				ShowPopularCourses = model.ShowPopularCourses,
				ShowCategories = model.ShowCategories,
				ShowLatestArticles = model.ShowLatestArticles,
				ShowPopularArticles = model.ShowPopularArticles,
				ShowStats = model.ShowStats,
				ShowTestimonials = model.ShowTestimonials,

				HomeFeaturedCoursesCount = model.HomeFeaturedCoursesCount,
				HomeLatestCoursesCount = model.HomeLatestCoursesCount,
				HomePopularCoursesCount = model.HomePopularCoursesCount,
				HomeArticlesCount = model.HomeArticlesCount,

				//ShowWhyChooseUs = model.ShowWhyChooseUs,
				//WhyChooseUsTitle = model.WhyChooseUsTitle,
				//WhyChooseUsSubtitle = model.WhyChooseUsSubtitle,
			};
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _siteService.UpdateSettingsAsync(dto, userId, cancellationToken);

			if (result != null)
			{
				TempData["Success"] = "تنظیمات با موفقیت ذخیره شد";
				return RedirectToAction("Settings");
			}

			TempData["Error"] = "خطا در ذخیره تنظیمات";
		}

		return View(model);
	}

	#endregion

	#region Dashboard Stats API

	[HttpGet]
	public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
	{
		var stats = await _adminService.GetDashboardStatsAsync(cancellationToken);
		return Json(stats);
	}

	#endregion

	// ============================= Instructor Requests =============================

	[HttpGet]
	public async Task<IActionResult> InstructorRequests(int page = 1, string? status = null, CancellationToken cancellationToken = default)
	{
		ViewBag.Title = "درخواست‌های مدرس شدن";

		var requestStatus = status?.ToLower() switch
		{
			"pending" => RequestStatus.Pending,
			"approved" => RequestStatus.Approved,
			"rejected" => RequestStatus.Rejected,
			_ => (RequestStatus?)null
		};

		var requests = await _instructorRequestService.GetRequestsAsync(page, 20, requestStatus, cancellationToken);
		ViewBag.CurrentStatus = status;

		return View(requests);
	}

	[HttpGet]
	public async Task<IActionResult> ViewInstructorRequest(Guid id, CancellationToken cancellationToken)
	{
		ViewBag.Title = "جزئیات درخواست";

		var request = await _instructorRequestService.GetRequestDetailsAsync(id, cancellationToken);
		if (request == null)
			return NotFound();

		return View(request);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ReviewInstructorRequest([FromQuery] Guid requestId, [FromQuery] bool isApproved, [FromQuery] string? rejectReason, CancellationToken cancellationToken)
    {
        ReviewInstructorRequestDto model = new ReviewInstructorRequestDto()
        {
            IsApproved = isApproved, RejectReason = rejectReason, RequestId = requestId
        };
		var adminId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
		var result = await _instructorRequestService.ReviewRequestAsync(model.RequestId, model.IsApproved, model.RejectReason, adminId, cancellationToken);

		if (result.Succeeded)
			TempData["Success"] = result.Message;
		else
			TempData["Error"] = result.Message;
        // اگر درخواست AJAX بود، JSON برگردان
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = result.Succeeded, message = result.Message });
        }
        return RedirectToAction("InstructorRequests");
	}

	#region Create Course
	// ==================== Course CRUD by Admin ====================

	[HttpGet]
	public async Task<IActionResult> CreateCourse(CancellationToken cancellationToken)
	{
		ViewBag.Title = "ایجاد دوره جدید";
		ViewBag.Instructors = await _adminService.GetAllUsersForSelectAsync(cancellationToken);
		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		ViewBag.Currency = (await _siteService.GetSettingsAsync(cancellationToken))?.CurrencySymbol ?? "تومان";
		return View(new CreateCourseByAdminDto());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateCourse(CreateCourseByAdminDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var adminId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _adminService.CreateCourseByAdminAsync(model, adminId, cancellationToken);

			if (result.Succeeded)
			{
				await _courseService.ClearCourseCacheAsync();
				TempData["Success"] = result.Message;
				return RedirectToAction("Courses");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Title = "ایجاد دوره جدید";
		ViewBag.Instructors = await _adminService.GetAllUsersForSelectAsync(cancellationToken);
		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		ViewBag.Currency = (await _siteService.GetSettingsAsync(cancellationToken))?.CurrencySymbol ?? "تومان";
		return View(model);
	}

	[HttpGet]
	public async Task<IActionResult> EditCourse(Guid id, CancellationToken cancellationToken)
	{
		ViewBag.Title = "ویرایش دوره";

		var course = await _adminService.GetCourseForAdminEditAsync(id, cancellationToken);
		if (course == null)
			return NotFound();

		ViewBag.Instructors = await _adminService.GetAllUsersForSelectAsync(cancellationToken);
		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		ViewBag.Currency = (await _siteService.GetSettingsAsync(cancellationToken))?.CurrencySymbol ?? "تومان";

		return View(course);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> EditCourse(UpdateCourseByAdminDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var adminId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _adminService.UpdateCourseByAdminAsync(model, adminId, cancellationToken);

			if (result.Succeeded)
			{
				await _courseService.ClearCourseCacheAsync(model.Id);
				TempData["Success"] = result.Message;
				return RedirectToAction("Courses");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Title = "ویرایش دوره";
		ViewBag.Instructors = await _adminService.GetAllUsersForSelectAsync(cancellationToken);
		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		ViewBag.Currency = (await _siteService.GetSettingsAsync(cancellationToken))?.CurrencySymbol ?? "تومان";
		return View(model);
	}
	#endregion
}