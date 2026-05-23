using HasibLms.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
        return Ok(categories);
    }
}