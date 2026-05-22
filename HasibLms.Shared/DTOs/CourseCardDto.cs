namespace HasibLms.Shared.DTOs;

public class CourseCardDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string ShortDescription { get; set; } = string.Empty;
	public string? ImageUrl { get; set; }
	public decimal Price { get; set; }
	public decimal? DiscountPrice { get; set; }
	public string InstructorName { get; set; } = string.Empty;
	public string InstructorAvatar { get; set; } = string.Empty;
	public string CategoryName { get; set; } = string.Empty;
	public int EnrolledCount { get; set; }
	public double AverageRating { get; set; }
	public int ReviewCount { get; set; }
	public DateTime? StartDate { get; set; }
	public bool IsPublished { get; set; }
}

public class CourseDetailDto : CourseCardDto
{
	public string Description { get; set; } = string.Empty;
	public string? IntroVideoUrl { get; set; }
	public int DurationHours { get; set; }
	public int Capacity { get; set; }
	public int RemainingCapacity { get; set; }
	public new DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public string CourseType { get; set; } = string.Empty;
	public bool IsUserEnrolled { get; set; }
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public List<SyllabusDto> Syllabuses { get; set; } = new();
	public List<ReviewDto> Reviews { get; set; } = new();
	public List<CourseCardDto> RelatedCourses { get; set; } = new();
}

public class SyllabusDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public int Order { get; set; }
	public List<LessonDto> Lessons { get; set; } = new();
}

public class LessonDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public int DurationMinutes { get; set; }
	public bool IsPreview { get; set; }
	public string? VideoUrl { get; set; }
	public int Order { get; set; }
}

public class ReviewDto
{
	public Guid Id { get; set; }
	public int Rating { get; set; }
	public string? Comment { get; set; }
	public string StudentName { get; set; } = string.Empty;
	public string? StudentAvatar { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsApproved { get; set; }
}