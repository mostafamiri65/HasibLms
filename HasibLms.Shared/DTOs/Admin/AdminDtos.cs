namespace HasibLms.Shared.DTOs.Admin;

public class UserListDto
{
	public Guid Id { get; set; }
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? PhoneNumber { get; set; }
	public List<string> Roles { get; set; } = new();
	public DateTime CreatedDate { get; set; }
	public bool IsLockedOut { get; set; }
	public bool EmailConfirmed { get; set; }
}

public class CourseListAdminDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public decimal? DiscountPrice { get; set; }
	public bool IsPublished { get; set; }
	public bool IsFeatured { get; set; }
	public string InstructorName { get; set; } = string.Empty;
	public string CategoryName { get; set; } = string.Empty;
	public int EnrolledCount { get; set; }
	public DateTime CreatedDate { get; set; }
	public DateTime? StartDate { get; set; }
}

public class ReviewListAdminDto
{
	public Guid Id { get; set; }
	public int Rating { get; set; }
	public string? Comment { get; set; }
	public bool IsApproved { get; set; }
	public string CourseTitle { get; set; } = string.Empty;
	public string StudentName { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}

public class DashboardStatsDto
{
	public int TotalUsers { get; set; }
	public int TotalStudents { get; set; }
	public int TotalInstructors { get; set; }
	public int TotalCourses { get; set; }
	public int PublishedCourses { get; set; }
	public int PendingCourses { get; set; }
	public int TotalEnrollments { get; set; }
	public int TotalArticles { get; set; }
	public int PendingReviews { get; set; }
	public decimal TotalRevenue { get; set; }
	public decimal MonthlyRevenue { get; set; }
	public List<MonthlyStatsDto> MonthlyEnrollments { get; set; } = new();
	public List<MonthlyStatsDto> MonthlyRevenueStats { get; set; } = new();
	public List<PopularCourseDto> PopularCourses { get; set; } = new();
}

public class MonthlyStatsDto
{
	public string Month { get; set; } = string.Empty;
	public int Count { get; set; }
	public decimal Amount { get; set; }
}

public class PopularCourseDto
{
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public int Enrollments { get; set; }
	public decimal Revenue { get; set; }
}