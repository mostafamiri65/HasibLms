using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Instructor;

namespace HasibLms.Domain.Interfaces;

public interface ICourseService
{
	// ============================= Public Methods =============================
	Task<CourseDetailDto?> GetCourseDetailAsync(Guid id, CancellationToken cancellationToken = default);
	Task<CourseDetailDto?> GetCourseBySlugAsync(string slug, CancellationToken cancellationToken = default);
	Task<PagedResultDto<CourseCardDto>> GetPagedCoursesAsync(int page, int pageSize, Guid? categoryId = null, string? searchTerm = null, string? sortBy = null, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<CourseCardDto>> GetFeaturedCoursesAsync(int count, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<CourseCardDto>> GetLatestCoursesAsync(int count, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<CourseCardDto>> GetPopularCoursesAsync(int count, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<CourseCardDto>> GetRelatedCoursesAsync(Guid courseId, int count, CancellationToken cancellationToken = default);
	Task<PagedResultDto<CourseCardDto>> SearchCoursesAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
	Task<HomePageDto> GetHomePageDataAsync(CancellationToken cancellationToken = default);

	// ============================= Instructor Methods =============================
	Task<PagedResultDto<CourseCardDto>> GetCoursesByInstructorAsync(Guid instructorId, int page, int pageSize, CancellationToken cancellationToken = default);
	Task<CourseEditDto?> GetCourseForEditAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> CreateCourseAsync(CreateCourseDto model, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdateCourseAsync(UpdateCourseDto model, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteCourseAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default);

	// ============================= Syllabus Management =============================
	Task<CourseDetailDto?> GetCourseWithSyllabusAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default);
	Task<SyllabusDetailDto?> GetSyllabusWithLessonsAsync(Guid syllabusId, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> AddSyllabusAsync(AddSyllabusDto model, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdateSyllabusAsync(UpdateSyllabusDto model, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteSyllabusAsync(Guid syllabusId, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ReorderSyllabusAsync(List<Guid> syllabusIds, Guid instructorId, CancellationToken cancellationToken = default);

	// ============================= Lesson Management =============================
	Task<AuthResultDto> AddLessonAsync(AddLessonDto model, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdateLessonAsync(UpdateLessonDto model, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteLessonAsync(Guid lessonId, Guid instructorId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ReorderLessonsAsync(List<Guid> lessonIds, Guid instructorId, CancellationToken cancellationToken = default);

	// ============================= Statistics =============================
	Task<CourseStatisticsDto?> GetCourseStatisticsAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default);

	// ============================= Admin Methods =============================
	//Task<PagedResultDto<CourseListAdminDto>> GetCoursesForAdminAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
	//Task<AuthResultDto> ApproveCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
	//Task<AuthResultDto> FeatureCourseAsync(Guid courseId, bool isFeatured, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteCourseByAdminAsync(Guid courseId, CancellationToken cancellationToken = default);

	// ============================= Cache Management =============================
	Task ClearCourseCacheAsync(Guid? courseId = null);
}