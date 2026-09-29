using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Shared.DTOs.Instructor;

namespace HasibLms.Domain.Interfaces;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<IReadOnlyList<Course>> GetPagedCoursesByInstructorAsync(Guid instructorId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Course?> GetCourseBySlug(string slug);
	Task<Course?> GetCourseWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Course>> GetFeaturedCoursesAsync(int count, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Course>> GetCoursesByCategoryAsync(Guid categoryId, int page, int pageSize, CancellationToken cancellationToken = default);
	Task<bool> IsUserEnrolledAsync(Guid courseId, Guid userId, CancellationToken cancellationToken = default);

	// متد جدید برای صفحه‌بندی با فیلترهای پیشرفته
	Task<IReadOnlyList<Course>> GetPagedCoursesAsync(
		int page,
		int pageSize,
		Guid? categoryId = null,
		string? searchTerm = null,
		string? sortBy = null,
		CancellationToken cancellationToken = default);
	Task<List<Course>> GetCoursesForAdmin(int page, int pageSize, string? status);
	Task<List<Syllabus>> GetSyllabusByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);

	Task<Syllabus?> GetSyllabusWithLessonsAsync(Guid syllabusId, Guid instructorId,
		CancellationToken cancellationToken = default);

	Task<List<Lesson>> GetLessonsBySyllabusId(Guid syllabusId, CancellationToken cancellationToken = default);
	Task<int> GetMaxOrderInSyllabus(Guid courseId);
	Task<int> GetMaxOrderInLesson(Guid syllabusId);
	Task<Syllabus?> GetSyllabusById(Guid syllabusId, CancellationToken cancellationToken = default);
	Task<Lesson?> GetLessonsByInstructorIdAndLessonIdAsync(Guid lessonId, Guid instructorId);
	Task<List<CourseReview>> GetCourseReviewsByCourseIdAsync(Guid courseId,CancellationToken cancellationToken=default);
}