namespace HasibLms.Shared.DTOs;

public class PagedResultDto<T>
{
	public List<T> Items { get; set; } = new();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
	public bool HasNextPage => PageNumber < TotalPages;
	public bool HasPreviousPage => PageNumber > 1;
}

public class CategoryDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid? ParentId { get; set; }
	public List<CategoryDto> Children { get; set; } = new();
}

public class TagDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
}