using HasibLms.Domain.Entities.AcademyEntities;

namespace HasibLms.Domain.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
	Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken cancellationToken = default);
	Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}