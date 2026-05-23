using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

[Authorize(Roles = "Instructor")]
public class InstructorController : Controller
{
	private readonly ICourseService _courseService;
	private readonly IEnrollmentService _enrollmentService;
	private readonly ICategoryService _categoryService;
	private readonly ILogger<InstructorController> _logger;

	public InstructorController(
		ICourseService courseService,
		IEnrollmentService enrollmentService,
		ICategoryService categoryService,
		ILogger<InstructorController> logger)
	{
		_courseService = courseService;
		_enrollmentService = enrollmentService;
		_categoryService = categoryService;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
	{
		ViewBag.Title = "داشبورد مدرس";
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

		var courses = await _courseService.GetCoursesByInstructorAsync(userId, 1, 100, cancellationToken);
		var totalStudents = 0;
		foreach (var course in courses.Items)
		{
			totalStudents += course.EnrolledCount;
		}

		ViewBag.TotalCourses = courses.TotalCount;
		ViewBag.TotalStudents = totalStudents;
		ViewBag.RecentCourses = courses.Items.Take(5).ToList();

		return View();
	}

	// ============================= Course Management =============================

	[HttpGet]
	public async Task<IActionResult> Courses(int page = 1, CancellationToken cancellationToken = default)
	{
		ViewBag.Title = "دوره‌های من";
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

		var courses = await _courseService.GetCoursesByInstructorAsync(userId, page, 10, cancellationToken);
		return View(courses);
	}

	[HttpGet]
	public async Task<IActionResult> CreateCourse(CancellationToken cancellationToken)
	{
		ViewBag.Title = "ایجاد دوره جدید";
		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateCourse(CreateCourseDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _courseService.CreateCourseAsync(model, userId, cancellationToken);

			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Courses");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		return View(model);
	}

	[HttpGet]
	public async Task<IActionResult> EditCourse(Guid id, CancellationToken cancellationToken)
	{
		ViewBag.Title = "ویرایش دوره";
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

		var course = await _courseService.GetCourseForEditAsync(id, userId, cancellationToken);
		if (course == null)
			return NotFound();

		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		return View(course);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> EditCourse(UpdateCourseDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
			var result = await _courseService.UpdateCourseAsync(model, userId, cancellationToken);

			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Courses");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken cancellationToken)
	{
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
		var result = await _courseService.DeleteCourseAsync(id, userId, cancellationToken);

		return Json(new { success = result.Succeeded, message = result.Message });
	}

	// ============================= Syllabus Management =============================

	[HttpGet]
	public async Task<IActionResult> Syllabus(Guid courseId, CancellationToken cancellationToken)
	{
		ViewBag.Title = "مدیریت سرفصل‌ها";
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

		var course = await _courseService.GetCourseWithSyllabusAsync(courseId, userId, cancellationToken);
		if (course == null)
			return NotFound();

		ViewBag.Course = course;
		return View(course);
	}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSyllabus([FromBody] AddSyllabusDto model, CancellationToken cancellationToken)
    {
        try
        {
            if ( model.CourseId == Guid.Empty)
            {
                return Json(new { success = false, message = "اطلاعات سرفصل معتبر نیست" });
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Json(new { success = false, message = "عنوان سرفصل الزامی است" });
            }

            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _courseService.AddSyllabusAsync(model, userId, cancellationToken);

            return Json(new { success = result.Succeeded, message = result.Message, syllabusId = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding syllabus");
            return Json(new { success = false, message = "خطا در افزودن سرفصل" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSyllabus([FromBody] UpdateSyllabusDto model, CancellationToken cancellationToken)
    {
        try
        {
            if ( model.Id == Guid.Empty)
            {
                return Json(new { success = false, message = "اطلاعات سرفصل معتبر نیست" });
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Json(new { success = false, message = "عنوان سرفصل الزامی است" });
            }

            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _courseService.UpdateSyllabusAsync(model, userId, cancellationToken);

            return Json(new { success = result.Succeeded, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating syllabus");
            return Json(new { success = false, message = "خطا در بروزرسانی سرفصل" });
        }
    }

    [HttpPost]
	public async Task<IActionResult> DeleteSyllabus(Guid syllabusId, CancellationToken cancellationToken)
	{
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
		var result = await _courseService.DeleteSyllabusAsync(syllabusId, userId, cancellationToken);

		return Json(new { success = result.Succeeded, message = result.Message });
	}

	[HttpPost]
	public async Task<IActionResult> ReorderSyllabus(List<Guid> syllabusIds, CancellationToken cancellationToken)
	{
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
		var result = await _courseService.ReorderSyllabusAsync(syllabusIds, userId, cancellationToken);

		return Json(new { success = result.Succeeded, message = result.Message });
	}

	// ============================= Lesson Management =============================

	[HttpGet]
	public async Task<IActionResult> Lessons(Guid syllabusId, CancellationToken cancellationToken)
	{
		ViewBag.Title = "مدیریت جلسات";
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

		var syllabus = await _courseService.GetSyllabusWithLessonsAsync(syllabusId, userId, cancellationToken);
		if (syllabus == null)
			return NotFound();

		ViewBag.Syllabus = syllabus;
		return View(syllabus);
	}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLesson([FromBody] AddLessonDto model, CancellationToken cancellationToken)
    {
        try
        {
            
            if (model.SyllabusId == Guid.Empty)
            {
                return Json(new { success = false, message = "شناسه سرفصل معتبر نیست" });
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Json(new { success = false, message = "عنوان جلسه الزامی است" });
            }

            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _courseService.AddLessonAsync(model, userId, cancellationToken);

            return Json(new { success = result.Succeeded, message = result.Message, lessonId = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding lesson");
            return Json(new { success = false, message = "خطا در افزودن جلسه: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLesson([FromBody] UpdateLessonDto model, CancellationToken cancellationToken)
    {
        try
        {
            if (model.Id == Guid.Empty)
            {
                return Json(new { success = false, message = "اطلاعات جلسه معتبر نیست" });
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Json(new { success = false, message = "عنوان جلسه الزامی است" });
            }

            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _courseService.UpdateLessonAsync(model, userId, cancellationToken);

            return Json(new { success = result.Succeeded, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson");
            return Json(new { success = false, message = "خطا در بروزرسانی جلسه" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        try
        {
            if ( lessonId == Guid.Empty)
            {
                return Json(new { success = false, message = "شناسه جلسه معتبر نیست" });
            }

            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _courseService.DeleteLessonAsync(lessonId, userId, cancellationToken);

            return Json(new { success = result.Succeeded, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lesson");
            return Json(new { success = false, message = "خطا در حذف جلسه" });
        }
    }

    [HttpPost]
	public async Task<IActionResult> ReorderLessons(List<Guid> lessonIds, CancellationToken cancellationToken)
	{
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
		var result = await _courseService.ReorderLessonsAsync(lessonIds, userId, cancellationToken);

		return Json(new { success = result.Succeeded, message = result.Message });
	}

	// ============================= Statistics =============================

	[HttpGet]
	public async Task<IActionResult> Statistics(Guid courseId, CancellationToken cancellationToken)
	{
		ViewBag.Title = "آمار دوره";
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

		var stats = await _courseService.GetCourseStatisticsAsync(courseId, userId, cancellationToken);
		if (stats == null)
			return NotFound();

		return View(stats);
	}
}