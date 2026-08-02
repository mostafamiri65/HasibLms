// AdminTagController.cs
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/tags")]
public class AdminTagController : Controller
{
    private readonly IAdminArticleService _articleService;
    private readonly ILogger<AdminTagController> _logger;

    public AdminTagController(IAdminArticleService articleService, ILogger<AdminTagController> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Title = "مدیریت تگ‌ها";

        var tags = await _articleService.GetAllTagsAsync(cancellationToken);
        return View(tags);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTagDto model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await _articleService.CreateTagAsync(model, cancellationToken);
            return Json(new { success = result.Succeeded, message = result.Message });
        }
        return Json(new { success = false, message = "اطلاعات وارد شده معتبر نیست" });
    }

    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateTagDto model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await _articleService.UpdateTagAsync(model, cancellationToken);
            return Json(new { success = result.Succeeded, message = result.Message });
        }
        return Json(new { success = false, message = "اطلاعات وارد شده معتبر نیست" });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _articleService.DeleteTagAsync(id, cancellationToken);
        return Json(new { success = result.Succeeded, message = result.Message });
    }
}