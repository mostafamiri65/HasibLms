using HasibLms.Shared.DTOs;

namespace HasibLms.Domain.Interfaces;

public interface ICategoryService
{
	Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
	Task<List<CategoryDto>> GetAllCategoriesWithChildrenAsync(CancellationToken cancellationToken = default);
	Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<Guid?> GetCategoryIdBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task ClearCategoryCacheAsync();
}