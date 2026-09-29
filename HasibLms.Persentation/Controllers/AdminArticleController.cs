// AdminArticleController.cs
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/articles")]
public class AdminArticleController : Controller
{
    private readonly IAdminArticleService _articleService;
    private readonly ILogger<AdminArticleController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private string RootPath;
    public AdminArticleController(
        IAdminArticleService articleService,
        ILogger<AdminArticleController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _articleService = articleService;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
        RootPath = _webHostEnvironment.WebRootPath;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, string? search = null, CancellationToken cancellationToken = default)
    {
        ViewBag.Title = "مدیریت مقالات";

        var articles = await _articleService.GetArticlesAsync(page, 20, search, cancellationToken);
        ViewBag.SearchTerm = search;

        return View(articles);
    }

	[IgnoreAntiforgeryToken]
	[HttpPost("upload-image")]
	[RequestSizeLimit(5 * 1024 * 1024)]
	public async Task<IActionResult> UploadImage(IFormFile upload)
	{
		if (upload == null || upload.Length == 0)
			return Json(new { uploaded = 0, error = new { message = "فایلی انتخاب نشده است" } });

		var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
		var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
		if (!allowedExtensions.Contains(ext))
			return Json(new { uploaded = 0, error = new { message = "فرمت فایل مجاز نیست" } });

		if (upload.Length > 5 * 1024 * 1024)
			return Json(new { uploaded = 0, error = new { message = "حجم فایل نباید بیشتر از ۵ مگابایت باشد" } });

		var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "articles");
		Directory.CreateDirectory(uploadsFolder);

		var fileName = $"{Guid.NewGuid():N}{ext}";
		var filePath = Path.Combine(uploadsFolder, fileName);

		using (var stream = new FileStream(filePath, FileMode.Create))
		{
			await upload.CopyToAsync(stream);
		}

		var url = $"/uploads/articles/{fileName}";

		// ✅ فرمت پاسخ مورد نیاز CKEditor 4
		return Json(new
		{
			uploaded = 1,
			fileName = fileName,
			url = url
		});
	}
	[HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Title = "ایجاد مقاله جدید";
        ViewBag.Tags = await _articleService.GetAllTagsAsync(cancellationToken);
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateArticleAdminDto model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _articleService.CreateArticleAsync(model,RootPath, userId, cancellationToken);

            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        ViewBag.Tags = await _articleService.GetAllTagsAsync(cancellationToken);
        return View(model);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        ViewBag.Title = "ویرایش مقاله";

        var article = await _articleService.GetArticleForEditAsync(id, cancellationToken);
        if (article == null)
            return NotFound();

        ViewBag.Tags = await _articleService.GetAllTagsAsync(cancellationToken);
        return View(article);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateArticleAdminDto model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await _articleService.UpdateArticleAsync(model,RootPath, cancellationToken);

            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        ViewBag.Tags = await _articleService.GetAllTagsAsync(cancellationToken);
        return View(model);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _articleService.DeleteArticleAsync(id,RootPath, cancellationToken);
        return Json(new { success = result.Succeeded, message = result.Message });
    }

    [HttpPost("toggle-publish")]
    public async Task<IActionResult> TogglePublish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _articleService.TogglePublishStatusAsync(id, cancellationToken);
        return Json(new { success = result.Succeeded, message = result.Message });
    }

    [HttpPost("toggle-feature")]
    public async Task<IActionResult> ToggleFeature(Guid id, bool isFeatured, CancellationToken cancellationToken)
    {
        var result = await _articleService.ToggleFeatureStatusAsync(id, isFeatured, cancellationToken);
        return Json(new { success = result.Succeeded, message = result.Message });
    }
}