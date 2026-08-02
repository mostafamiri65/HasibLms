// ArticleListAdminDto.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HasibLms.Shared.DTOs.Admin;

public class CreateArticleAdminDto
{
    [Required(ErrorMessage = "عنوان مقاله الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیشتر از 200 کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "خلاصه نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string? Summary { get; set; }

    [Required(ErrorMessage = "متن مقاله الزامی است")]
    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public List<string> TagNames { get; set; } = new();
}

// UpdateArticleAdminDto.cs
public class UpdateArticleAdminDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "عنوان مقاله الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیشتر از 200 کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "خلاصه نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string? Summary { get; set; }

    [Required(ErrorMessage = "متن مقاله الزامی است")]
    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public List<string> TagNames { get; set; } = new();
    public bool RemoveImage { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

// ArticleListAdminDto.cs
public class ArticleListAdminDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string StatusText => IsPublished ? "منتشر شده" : "پیش‌نویس";
    public string StatusColor => IsPublished ? "green" : "yellow";
}

// ArticleDetailAdminDto.cs
public class ArticleDetailAdminDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<TagDto> AllTags { get; set; } = new();
}

// TagListDto.cs
public class TagListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int ArticleCount { get; set; }
}

// CreateTagDto.cs
public class CreateTagDto
{
    [Required(ErrorMessage = "نام تگ الزامی است")]
    [MaxLength(50, ErrorMessage = "نام تگ نمی‌تواند بیشتر از 50 کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;
}

public class UpdateTagDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "نام تگ الزامی است")]
    [MaxLength(50, ErrorMessage = "نام تگ نمی‌تواند بیشتر از 50 کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;
}