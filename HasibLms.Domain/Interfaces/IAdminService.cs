using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Instructor;

namespace HasibLms.Domain.Interfaces;

public interface IAdminService
{
	// ==================== User Management ====================
	Task<PagedResultDto<UserListDto>> GetUsersAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default);
	Task<AuthResultDto> AssignRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
	Task<AuthResultDto> RemoveRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ToggleUserStatusAsync(Guid userId, CancellationToken cancellationToken = default);

	// ==================== Course Management ====================
	Task<AuthResultDto> ApproveCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UnPublishCourse(Guid courseId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> FeatureCourseAsync(Guid courseId, bool isFeatured, CancellationToken cancellationToken = default);
	Task<PagedResultDto<CourseListAdminDto>> GetCoursesForAdminAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

	// ==================== Course CRUD by Admin ====================
	/// <summary>
	/// لیست تمام کاربران (برای انتخاب مدرس در فرم ایجاد دوره)
	/// </summary>
	Task<List<InstructorSelectDto>> GetAllUsersForSelectAsync(CancellationToken ct = default);

	/// <summary>
	/// ایجاد دوره توسط ادمین برای یک کاربر (اگر کاربر مدرس نباشد، نقش مدرس داده می‌شود)
	/// </summary>
	Task<AuthResultDto> CreateCourseByAdminAsync(CreateCourseByAdminDto model, Guid adminId, CancellationToken ct = default);

	/// <summary>
	/// ویرایش دوره توسط ادمین
	/// </summary>
	Task<AuthResultDto> UpdateCourseByAdminAsync(UpdateCourseByAdminDto model, Guid adminId, CancellationToken ct = default);

	/// <summary>
	/// دریافت اطلاعات دوره برای ویرایش توسط ادمین
	/// </summary>
	Task<UpdateCourseByAdminDto?> GetCourseForAdminEditAsync(Guid courseId, CancellationToken ct = default);

	// ==================== Review Management ====================
	Task<AuthResultDto> ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
	Task<PagedResultDto<ReviewListAdminDto>> GetReviewsForAdminAsync(int page, int pageSize, bool? pending = null, CancellationToken cancellationToken = default);

	// ==================== Dashboard Stats ====================
	Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}