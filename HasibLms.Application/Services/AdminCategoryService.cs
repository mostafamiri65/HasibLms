using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HasibLms.Application.Services;

public class AdminCategoryService : IAdminCategoryService
{
	private readonly ICategoryRepository _categoryRepository;
	private readonly IGenericRepository<Course> _courseRepository;
	private readonly ILogger<AdminCategoryService> _logger;

	public AdminCategoryService(
		ICategoryRepository categoryRepository,
		IGenericRepository<Course> courseRepository,
		ILogger<AdminCategoryService> logger)
	{
		_categoryRepository = categoryRepository;
		_courseRepository = courseRepository;
		_logger = logger;
	}

	public async Task<List<CategoryListAdminDto>> GetAllCategoriesAsync(CancellationToken ct = default)
	{
		var categories = await _categoryRepository.GetAllWithChildrenAsync(ct);
		var result = new List<CategoryListAdminDto>();

		foreach (var category in categories.OrderBy(c => c.Name))
		{
			var courseCount = await _courseRepository.CountAsync(c => c.CategoryId == category.Id, ct);
			result.Add(new CategoryListAdminDto
			{
				Id = category.Id,
				Name = category.Name,
				Slug = category.Slug,
				Description = category.Description,
				ParentId = category.ParentId,
				ParentName = category.Parent?.Name,
				CourseCount = courseCount,
				ChildrenCount = category.Children?.Count ?? 0,
				CreatedDate = category.CreatedDate
			});
		}

		return result;
	}

	public async Task<List<CategoryListAdminDto>> GetAllCategoriesExceptAsync(Guid? excludeId = null, CancellationToken ct = default)
	{
		var all = await GetAllCategoriesAsync(ct);
		if (excludeId.HasValue)
		{
			// حذف خود دسته و زیرمجموعه‌هایش
			var toRemove = new HashSet<Guid> { excludeId.Value };
			var children = all.Where(c => c.ParentId.HasValue && toRemove.Contains(c.ParentId.Value)).Select(c => c.Id).ToList();
			foreach (var cid in children) toRemove.Add(cid);
			return all.Where(c => !toRemove.Contains(c.Id)).ToList();
		}
		return all;
	}

	public async Task<CategoryListAdminDto?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default)
	{
		var category = await _categoryRepository.GetSingleAsync(c => c.Id == id, ct);
		if (category == null) return null;

		var parent = category.ParentId.HasValue
			? await _categoryRepository.GetSingleAsync(c => c.Id == category.ParentId.Value, ct)
			: null;

		var courseCount = await _courseRepository.CountAsync(c => c.CategoryId == category.Id, ct);
		var children = await _categoryRepository.GetAsync(c => c.ParentId == category.Id, ct);

		return new CategoryListAdminDto
		{
			Id = category.Id,
			Name = category.Name,
			Slug = category.Slug,
			Description = category.Description,
			ParentId = category.ParentId,
			ParentName = parent?.Name,
			CourseCount = courseCount,
			ChildrenCount = children.Count,
			CreatedDate = category.CreatedDate
		};
	}

	public async Task<AuthResultDto> CreateCategoryAsync(CreateCategoryDto model, CancellationToken ct = default)
	{
		try
		{
			var slug = !string.IsNullOrWhiteSpace(model.Slug)
				? GenerateSlug(model.Slug)
				: GenerateSlug(model.Name);

			if (string.IsNullOrEmpty(slug))
				slug = $"category-{Guid.NewGuid():N}".Substring(0, 20);

			// چک تکراری بودن
			var baseSlug = slug;
			int counter = 1;
			while (await _categoryRepository.ExistsAsync(c => c.Slug == slug, ct))
			{
				slug = $"{baseSlug}-{counter}";
				counter++;
			}

			// بررسی وجود والد
			if (model.ParentId.HasValue)
			{
				var parentExists = await _categoryRepository.ExistsAsync(c => c.Id == model.ParentId.Value, ct);
				if (!parentExists)
					return new AuthResultDto { Succeeded = false, Message = "دسته‌بندی والد یافت نشد" };
			}

			var category = new Category
			{
				Id = Guid.NewGuid(),
				Name = model.Name.Trim(),
				Slug = slug,
				Description = model.Description?.Trim(),
				ParentId = model.ParentId,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _categoryRepository.AddAsync(category, ct);
			_logger.LogInformation("Category created: {Name}", model.Name);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "دسته‌بندی با موفقیت ایجاد شد",
				Data = category.Id
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating category");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ایجاد دسته‌بندی",
				Errors = new List<string> { ex.Message }
			};
		}
	}

	public async Task<AuthResultDto> UpdateCategoryAsync(UpdateCategoryDto model, CancellationToken ct = default)
	{
		try
		{
			var category = await _categoryRepository.GetByIdAsync(model.Id, ct);
			if (category == null)
				return new AuthResultDto { Succeeded = false, Message = "دسته‌بندی یافت نشد" };

			// جلوگیری از انتخاب خود به عنوان والد
			if (model.ParentId.HasValue && model.ParentId.Value == model.Id)
				return new AuthResultDto { Succeeded = false, Message = "یک دسته‌بندی نمی‌تواند والد خودش باشد" };

			// جلوگیری از حلقه (اگر والد یکی از زیرمجموعه‌ها باشد)
			if (model.ParentId.HasValue)
			{
				var parent = await _categoryRepository.GetByIdAsync(model.ParentId.Value, ct);
				if (parent == null)
					return new AuthResultDto { Succeeded = false, Message = "دسته‌بندی والد یافت نشد" };

				var isDescendant = await IsDescendantAsync(parent.Id, model.Id, ct);
				if (isDescendant)
					return new AuthResultDto { Succeeded = false, Message = "نمی‌توانید یک زیردسته را به عنوان والد انتخاب کنید" };
			}

			// چک slug تکراری
			var slug = !string.IsNullOrWhiteSpace(model.Slug)
				? GenerateSlug(model.Slug)
				: GenerateSlug(model.Name);

			if (string.IsNullOrEmpty(slug))
				slug = $"category-{Guid.NewGuid():N}".Substring(0, 20);

			var baseSlug = slug;
			int counter = 1;
			while (await _categoryRepository.ExistsAsync(c => c.Slug == slug && c.Id != model.Id, ct))
			{
				slug = $"{baseSlug}-{counter}";
				counter++;
			}

			category.Name = model.Name.Trim();
			category.Slug = slug;
			category.Description = model.Description?.Trim();
			category.ParentId = model.ParentId;
			category.LastModifiedDate = DateTime.UtcNow;

			await _categoryRepository.UpdateAsync(category, ct);
			_logger.LogInformation("Category updated: {Id}", model.Id);

			return new AuthResultDto { Succeeded = true, Message = "دسته‌بندی با موفقیت بروزرسانی شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating category {Id}", model.Id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بروزرسانی دسته‌بندی",
				Errors = new List<string> { ex.Message }
			};
		}
	}

	public async Task<AuthResultDto> DeleteCategoryAsync(Guid id, CancellationToken ct = default)
	{
		try
		{
			var category = await _categoryRepository.GetByIdAsync(id, ct);
			if (category == null)
				return new AuthResultDto { Succeeded = false, Message = "دسته‌بندی یافت نشد" };

			// بررسی وابستگی‌ها
			var courseCount = await _courseRepository.CountAsync(c => c.CategoryId == id, ct);
			if (courseCount > 0)
				return new AuthResultDto
				{
					Succeeded = false,
					Message = $"این دسته‌بندی دارای {courseCount} دوره است و قابل حذف نیست. ابتدا دوره‌ها را جابجا کنید."
				};

			var children = await _categoryRepository.GetAsync(c => c.ParentId == id, ct);
			if (children.Any())
				return new AuthResultDto
				{
					Succeeded = false,
					Message = $"این دسته‌بندی دارای {children.Count} زیردسته است و قابل حذف نیست. ابتدا زیردسته‌ها را حذف یا جابجا کنید."
				};

			await _categoryRepository.DeleteAsync(category, ct);
			_logger.LogInformation("Category deleted: {Id}", id);

			return new AuthResultDto { Succeeded = true, Message = "دسته‌بندی با موفقیت حذف شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting category {Id}", id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در حذف دسته‌بندی",
				Errors = new List<string> { ex.Message }
			};
		}
	}

	// ==================== Helpers ====================

	private async Task<bool> IsDescendantAsync(Guid potentialDescendantId, Guid ancestorId, CancellationToken ct)
	{
		// آیا potentialDescendant زیرمجموعه ancestor است؟
		var currentId = potentialDescendantId;
		int safety = 0;
		while (currentId != Guid.Empty && safety < 20)
		{
			var current = await _categoryRepository.GetByIdAsync(currentId, ct);
			if (current == null) return false;
			if (current.ParentId == ancestorId) return true;
			if (current.ParentId == null) return false;
			currentId = current.ParentId.Value;
			safety++;
		}
		return false;
	}

	private static string GenerateSlug(string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return string.Empty;
		var slug = text.Trim().ToLowerInvariant();
		slug = slug.Replace("ي", "ی").Replace("ك", "ک");
		slug = slug.Replace(" ", "-");
		slug = Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF\-]", "");
		slug = Regex.Replace(slug, @"-{2,}", "-");
		return slug.Trim('-');
	}
}