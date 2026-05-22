using HasibLms.Domain.Entities;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Instructor;
using HasibLms.Shared.Enumerations;

namespace HasibLms.Domain.Interfaces;

public interface IInstructorRequestService
{
	Task<AuthResultDto> ApplyForInstructorAsync(ApplyForInstructorDto model, Guid userId, CancellationToken cancellationToken = default);
	Task<PagedResultDto<InstructorRequestListDto>> GetRequestsAsync(int page, int pageSize, RequestStatus? status = null, CancellationToken cancellationToken = default);
	Task<InstructorRequestListDto?> GetRequestDetailsAsync(Guid requestId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ReviewRequestAsync(Guid requestId, bool isApproved, string? rejectReason, Guid adminId, CancellationToken cancellationToken = default);
	Task<bool> CanApplyForInstructorAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<InstructorRequest?> GetUserActiveRequestAsync(Guid userId, CancellationToken cancellationToken = default);
}