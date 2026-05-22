namespace HasibLms.Shared.DTOs;

public class HomePageDto
{
	public List<CourseCardDto> FeaturedCourses { get; set; } = new();
	public List<CourseCardDto> LatestCourses { get; set; } = new();
	public List<CourseCardDto> PopularCourses { get; set; } = new();
	public List<CategoryDto> Categories { get; set; } = new();
	public List<ArticleCardDto> LatestArticles { get; set; } = new();
	public List<ArticleCardDto> PopularArticles { get; set; } = new();
	public HomeStatsDto Stats { get; set; } = new();
}

public class HomeStatsDto
{
	public int TotalStudents { get; set; }
	public int TotalCourses { get; set; }
	public int TotalInstructors { get; set; }
	public int TotalArticles { get; set; }
}