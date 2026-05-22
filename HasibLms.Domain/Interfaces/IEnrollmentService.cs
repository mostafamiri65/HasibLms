using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;

namespace HasibLms.Domain.Interfaces;

public interface IEnrollmentService
{
	Task<AuthResultDto> EnrollInCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> CancelEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
	Task<bool> IsUserEnrolledAsync(Guid courseId, Guid userId, CancellationToken cancellationToken = default);
	Task<List<CourseCardDto>> GetUserEnrolledCoursesAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<int> GetUserEnrollmentCountAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<int> GetCourseEnrollmentCountAsync(Guid courseId, CancellationToken cancellationToken = default);
}