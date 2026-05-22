using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;

namespace HasibLms.Domain.Interfaces;

public interface IReviewService
{
	Task<AuthResultDto> AddReviewAsync(Guid userId, Guid courseId, int rating, string? comment, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
	Task<PagedResultDto<ReviewDto>> GetCourseReviewsAsync(Guid courseId, int page, int pageSize, CancellationToken cancellationToken = default);
	Task<double> GetCourseAverageRatingAsync(Guid courseId, CancellationToken cancellationToken = default);
	Task ClearReviewCacheAsync(Guid? courseId = null);
}