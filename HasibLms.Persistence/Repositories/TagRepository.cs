using HasibLms.Domain.Entities.InvestigativeEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;

namespace HasibLms.Persistence.Repositories;

public class TagRepository : GenericRepository<Tag>,ITagRepository
{
	public TagRepository(LmsContext context) : base(context)
	{
	}
}