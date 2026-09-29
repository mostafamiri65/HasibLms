using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/why-choose-us")]
public class AdminWhyChooseUsController : Controller
{
	private readonly IWhyChooseUsService _service;
	private readonly ILogger<AdminWhyChooseUsController> _logger;

	public AdminWhyChooseUsController(IWhyChooseUsService service, ILogger<AdminWhyChooseUsController> logger)
	{
		_service = service;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> Index(CancellationToken cancellationToken)
	{
		ViewBag.Title = "مدیریت بخش 'چرا ما؟'";
		var settings = await _service.GetSettingsAsync(cancellationToken);
		return View(settings);
	}

	[HttpGet("create")]
	public IActionResult Create()
	{
		ViewBag.Title = "ایجاد آیتم جدید";
		return View(new CreateWhyChooseUsItemDto());
	}

	[HttpPost("create")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateWhyChooseUsItemDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var result = await _service.CreateItemAsync(model, cancellationToken);
			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Index");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Title = "ایجاد آیتم جدید";
		return View(model);
	}

	[HttpGet("edit/{id}")]
	public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
	{
		ViewBag.Title = "ویرایش آیتم";
		var item = await _service.GetItemByIdAsync(id, cancellationToken);
		if (item == null)
			return NotFound();

		var model = new UpdateWhyChooseUsItemDto
		{
			Id = item.Id,
			Title = item.Title,
			Description = item.Description,
			Icon = item.Icon,
			IsActive = item.IsActive,
			ShowOnHomePage = item.ShowOnHomePage,
			BackgroundColor = item.BackgroundColor,
			IconColor = item.IconColor
		};

		return View(model);
	}

	[HttpPost("edit")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(UpdateWhyChooseUsItemDto model, CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var result = await _service.UpdateItemAsync(model, cancellationToken);
			if (result.Succeeded)
			{
				TempData["Success"] = result.Message;
				return RedirectToAction("Index");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Title = "ویرایش آیتم";
		return View(model);
	}

	[HttpPost("delete")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var result = await _service.DeleteItemAsync(id, cancellationToken);
		return Json(new { success = result.Succeeded, message = result.Message });
	}

	[HttpPost("reorder")]
	public async Task<IActionResult> Reorder([FromBody] List<Guid> ids, CancellationToken cancellationToken)
	{
		var result = await _service.ReorderItemsAsync(ids, cancellationToken);
		return Json(new { success = result.Succeeded, message = result.Message });
	}

	[HttpPost("toggle-section")]
	public async Task<IActionResult> ToggleSection(bool show, CancellationToken cancellationToken)
	{
		var result = await _service.ToggleShowSectionAsync(show, cancellationToken);
		return Json(new { success = result.Succeeded, message = result.Message });
	}
}