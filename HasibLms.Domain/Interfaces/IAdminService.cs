using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;

namespace HasibLms.Domain.Interfaces;

public interface IAdminService
{
	// User Management
	Task<PagedResultDto<UserListDto>> GetUsersAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default);
	Task<AuthResultDto> AssignRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
	Task<AuthResultDto> RemoveRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ToggleUserStatusAsync(Guid userId, CancellationToken cancellationToken = default);

	// Course Management
	Task<AuthResultDto> ApproveCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> FeatureCourseAsync(Guid courseId, bool isFeatured, CancellationToken cancellationToken = default);

	// Review Management
	Task<AuthResultDto> ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);

	// Dashboard Stats
	Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
	// به IAdminService اضافه کنید:
	Task<PagedResultDto<CourseListAdminDto>> GetCoursesForAdminAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
	Task<PagedResultDto<ReviewListAdminDto>> GetReviewsForAdminAsync(int page, int pageSize, bool? pending = null, CancellationToken cancellationToken = default);
}