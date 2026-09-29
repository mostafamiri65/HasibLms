using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/categories")]
public class AdminCategoryController : Controller
{
	private readonly IAdminCategoryService _categoryService;
	private readonly ILogger<AdminCategoryController> _logger;

	public AdminCategoryController(
		IAdminCategoryService categoryService,
		ILogger<AdminCategoryController> logger)
	{
		_categoryService = categoryService;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> Index(CancellationToken ct)
	{
		ViewBag.Title = "مدیریت دسته‌بندی‌ها";
		var categories = await _categoryService.GetAllCategoriesAsync(ct);
		return View(categories);
	}

	[HttpPost("create")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateCategoryDto model, CancellationToken ct)
	{
		if (!ModelState.IsValid)
		{
			var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
			return Json(new { success = false, message = string.Join(" | ", errors) });
		}

		var result = await _categoryService.CreateCategoryAsync(model, ct);
		return Json(new { success = result.Succeeded, message = result.Message });
	}

	[HttpPost("update")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Update(UpdateCategoryDto model, CancellationToken ct)
	{
		if (!ModelState.IsValid)
		{
			var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
			return Json(new { success = false, message = string.Join(" | ", errors) });
		}

		var result = await _categoryService.UpdateCategoryAsync(model, ct);
		return Json(new { success = result.Succeeded, message = result.Message });
	}

	[HttpPost("delete")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		var result = await _categoryService.DeleteCategoryAsync(id, ct);
		return Json(new { success = result.Succeeded, message = result.Message });
	}

	[HttpGet("get/{id}")]
	public async Task<IActionResult> Get(Guid id, CancellationToken ct)
	{
		var category = await _categoryService.GetCategoryByIdAsync(id, ct);
		if (category == null)
			return Json(new { success = false, message = "دسته‌بندی یافت نشد" });

		return Json(new
		{
			success = true,
			data = new
			{
				id = category.Id,
				name = category.Name,
				slug = category.Slug,
				description = category.Description,
				parentId = category.ParentId
			}
		});
	}

	[HttpGet("all")]
	public async Task<IActionResult> GetAll(CancellationToken ct)
	{
		var categories = await _categoryService.GetAllCategoriesAsync(ct);
		return Json(new
		{
			success = true,
			data = categories.Select(c => new { id = c.Id, name = c.Name })
		});
	}

	[HttpGet("available-parents")]
	public async Task<IActionResult> GetAvailableParents(Guid? excludeId, CancellationToken ct)
	{
		var categories = await _categoryService.GetAllCategoriesExceptAsync(excludeId, ct);
		return Json(new
		{
			success = true,
			data = categories.Select(c => new { id = c.Id, name = c.Name })
		});
	}
}