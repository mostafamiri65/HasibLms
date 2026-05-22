using HasibLms.Domain.Entities;

namespace HasibLms.Domain.Interfaces;

public interface IInstructorRequestRepository : IGenericRepository<InstructorRequest>
{
	Task<InstructorRequest?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<bool> HasPendingRequestAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<InstructorRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
}