using HasibLms.Domain.Entities;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using HasibLms.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class InstructorRequestRepository : GenericRepository<InstructorRequest>, IInstructorRequestRepository
{
	public InstructorRequestRepository(LmsContext context) : base(context)
	{
	}

	public async Task<InstructorRequest?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.FirstOrDefaultAsync(r => r.UserId == userId && !r.IsDeleted, cancellationToken);
	}

	public async Task<bool> HasPendingRequestAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.AnyAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending && !r.IsDeleted, cancellationToken);
	}

	public async Task<IReadOnlyList<InstructorRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(r => r.Status == RequestStatus.Pending && !r.IsDeleted)
			.OrderByDescending(r => r.RequestedAt)
			.ToListAsync(cancellationToken);
	}
}