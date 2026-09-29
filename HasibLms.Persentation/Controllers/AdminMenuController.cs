using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/menu")]
public class AdminMenuController : Controller
{
	private readonly IMenuService _menuService;
	private readonly ILogger<AdminMenuController> _logger;

	public AdminMenuController(IMenuService menuService, ILogger<AdminMenuController> logger)
	{
		_menuService = menuService;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> Index(CancellationToken cancellationToken)
	{
		ViewBag.Title = "مدیریت منوها";
		var settings = await _menuService.GetMenuSettingsAsync(cancellationToken);
		return View(settings);
	}

	[HttpGet("create")]
	public async Task<IActionResult> Create(string location = "header")
	{
		ViewBag.Title = "ایجاد منوی جدید";
		ViewBag.Location = location;

		// دریافت لیست منوهای موجود برای انتخاب والد
		var allItems = await _menuService.GetMenuSettingsAsync();
		var parentItems = location == "header"
			? allItems.HeaderMenuItems
			: allItems.FooterMenuItems;
		ViewBag.ParentItems = parentItems;
		return View(new CreateMenuItemDto { Location = location });
	}

	[HttpPost("create")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateMenuItemDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var result = await _menuService.CreateMenuItemAsync(model, cancellationToken);
			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Index");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Title = "ایجاد منوی جدید";
		return View(model);
	}

	[HttpGet("edit/{id}")]
	public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
	{
		ViewBag.Title = "ویرایش منو";
		var item = await _menuService.GetMenuItemByIdAsync(id, cancellationToken);
		if (item == null)
			return NotFound();

		// دریافت لیست منوهای موجود برای انتخاب والد
		var allItems = await _menuService.GetMenuSettingsAsync(cancellationToken);
		var parentItems = item.Location == "header"
			? allItems.HeaderMenuItems
			: allItems.FooterMenuItems;
		// حذف خود آیتم از لیست والدها
		parentItems = parentItems.Where(x => x.Id != id).ToList();
		ViewBag.ParentItems = parentItems;

		var model = new UpdateMenuItemDto
		{
			Id = item.Id,
			Title = item.Title,
			Url = item.Url,
			Icon = item.Icon,
			Target = item.Target,
			CssClass = item.CssClass,
			ParentId = item.ParentId,
			IsActive = item.IsActive,
			Location = item.Location
		};

		return View(model);
	}

	[HttpPost("edit")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(UpdateMenuItemDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var result = await _menuService.UpdateMenuItemAsync(model, cancellationToken);
			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Index");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Title = "ویرایش منو";
		return View(model);
	}

	[HttpPost("delete")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var result = await _menuService.DeleteMenuItemAsync(id, cancellationToken);
		return Json(new { success = result.Succeeded, message = result.Message });
	}

	[HttpPost("reorder")]
	public async Task<IActionResult> Reorder(string location, [FromBody] List<Guid> ids, CancellationToken cancellationToken)
	{
		var result = await _menuService.ReorderMenuItemsAsync(location, ids, cancellationToken);
		return Json(new { success = result.Succeeded, message = result.Message });
	}
}