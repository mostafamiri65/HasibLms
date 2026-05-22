using HasibLms.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

public class CourseController : Controller
{
	private readonly ICourseService _courseService;
	private readonly ICategoryService _categoryService;
	private readonly IEnrollmentService _enrollmentService;
	private readonly ISiteService _siteService;
	private readonly ILogger<CourseController> _logger;

	public CourseController(
		ICourseService courseService,
		ICategoryService categoryService,
		IEnrollmentService enrollmentService,
		ISiteService siteService,
		ILogger<CourseController> logger)
	{
		_courseService = courseService;
		_categoryService = categoryService;
		_enrollmentService = enrollmentService;
		_siteService = siteService;
		_logger = logger;
	}

	[ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "category", "sort", "search" })]
	public async Task<IActionResult> Index(
		int page = 1,
		string? category = null,
		string? sort = "latest",
		string? search = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var settings = await _siteService.GetSettingsAsync(cancellationToken);
			ViewBag.Title = "دوره‌های آموزشی";
			ViewBag.MetaDescription = $"دوره‌های تخصصی {settings?.InstituteName} در زمینه حسابداری، مالیات، سرمایه‌گذاری و بودجه‌ریزی";

			var categoryId = !string.IsNullOrEmpty(category)
				? await _categoryService.GetCategoryIdBySlugAsync(category, cancellationToken)
				: null;

			var result = await _courseService.GetPagedCoursesAsync(page, 12, categoryId,search,sort, cancellationToken);

			// Apply sorting
			var items = result.Items.AsEnumerable();
			items = sort switch
			{
				"price_asc" => items.OrderBy(c => c.Price),
				"price_desc" => items.OrderByDescending(c => c.Price),
				"popular" => items.OrderByDescending(c => c.EnrolledCount),
				"rating" => items.OrderByDescending(c => c.AverageRating),
				"title_asc" => items.OrderBy(c => c.Title),
				"title_desc" => items.OrderByDescending(c => c.Title),
				_ => items.OrderByDescending(c => c.Id)
			};

			if (!string.IsNullOrEmpty(search))
			{
				items = items.Where(c =>
					c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
					c.ShortDescription.Contains(search, StringComparison.OrdinalIgnoreCase) ||
					c.InstructorName.Contains(search, StringComparison.OrdinalIgnoreCase));
				ViewBag.SearchTerm = search;
			}

			result.Items = items.ToList();

			ViewBag.CurrentCategory = category;
			ViewBag.CurrentSort = sort;
			ViewBag.CurrentSearch = search;
			ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);

			return View(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading courses page");
			return View("Error");
		}
	}

	[ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "slug" })]
	public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
	{
		try
		{
			if (string.IsNullOrEmpty(slug))
				return NotFound();

			var model = await _courseService.GetCourseBySlugAsync(slug, cancellationToken);
			if (model == null)
				return NotFound();

			ViewBag.Title = model.Title;
			ViewBag.MetaDescription = model.ShortDescription;
			ViewBag.MetaKeywords = $"{model.Title}, آموزش, {model.CategoryName}, دوره آنلاین";
			ViewBag.OgImage = model.ImageUrl;

			// Check if user is enrolled
			if (User.Identity?.IsAuthenticated == true)
			{
				var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
				model.IsUserEnrolled = await _enrollmentService.IsUserEnrolledAsync(model.Id, userId, cancellationToken);
			}

			// Get related courses (same category)
			var related = await _courseService.GetRelatedCoursesAsync(model.Id, 4, cancellationToken);
			ViewBag.RelatedCourses = related;

			return View(model);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading course detail for slug: {Slug}", slug);
			return View("Error");
		}
	}

	[ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "slug", "page" })]
	public async Task<IActionResult> ByCategory(string slug, int page = 1, CancellationToken cancellationToken = default)
	{
		try
		{
			if (string.IsNullOrEmpty(slug))
				return NotFound();

			var category = await _categoryService.GetCategoryBySlugAsync(slug, cancellationToken);
			if (category == null)
				return NotFound();

			ViewBag.Title = $"دوره‌های {category.Name}";
			ViewBag.MetaDescription = category.Name ?? $"دوره‌های تخصصی {category.Name} در {await _siteService.GetInstituteNameAsync()}";
			ViewBag.CategoryName = category.Name;

			var result = await _courseService.GetPagedCoursesAsync(page:page,pageSize: 12,categoryId: category.Id,cancellationToken: cancellationToken);

			ViewBag.Category = category;
			ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);

			return View(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading category page for slug: {Slug}", slug);
			return View("Error");
		}
	}

	[Authorize]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Enroll(Guid courseId, CancellationToken cancellationToken)
	{
		try
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

			var result = await _enrollmentService.EnrollInCourseAsync(userId, courseId, cancellationToken);

			if (result.Succeeded)
			{
				// Clear course cache
				await _courseService.ClearCourseCacheAsync(courseId);
				return Json(new { success = true, message = result.Message });
			}

			return Json(new { success = false, message = result.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error enrolling user in course {CourseId}", courseId);
			return Json(new { success = false, message = "خطا در ثبت نام" });
		}
	}

	[Authorize]
	[HttpGet]
	public async Task<IActionResult> MyCourses(CancellationToken cancellationToken)
	{
		try
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

			var courses = await _enrollmentService.GetUserEnrolledCoursesAsync(userId, cancellationToken);

			ViewBag.Title = "دوره‌های من";
			return View(courses);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading my courses");
			return View("Error");
		}
	}

	[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "q", "page" })]
	public async Task<IActionResult> Search(string q, int page = 1, CancellationToken cancellationToken = default)
	{
		try
		{
			if (string.IsNullOrEmpty(q))
				return RedirectToAction("Index");

			ViewBag.Title = $"جستجو: {q}";
			ViewBag.SearchTerm = q;

			var result = await _courseService.SearchCoursesAsync(q, page, 12, cancellationToken);

			return View(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error searching for {Query}", q);
			return View("Error");
		}
	}
}