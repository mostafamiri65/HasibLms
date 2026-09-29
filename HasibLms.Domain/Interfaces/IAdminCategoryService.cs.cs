using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;

namespace HasibLms.Domain.Interfaces;

public interface IAdminCategoryService
{
	Task<List<CategoryListAdminDto>> GetAllCategoriesAsync(CancellationToken ct = default);
	Task<CategoryListAdminDto?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default);
	Task<AuthResultDto> CreateCategoryAsync(CreateCategoryDto model, CancellationToken ct = default);
	Task<AuthResultDto> UpdateCategoryAsync(UpdateCategoryDto model, CancellationToken ct = default);
	Task<AuthResultDto> DeleteCategoryAsync(Guid id, CancellationToken ct = default);
	Task<List<CategoryListAdminDto>> GetAllCategoriesExceptAsync(Guid? excludeId = null, CancellationToken ct = default);
}