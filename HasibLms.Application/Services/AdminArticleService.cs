// AdminArticleService.cs
using HasibLms.Domain.Entities.InvestigativeEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class AdminArticleService : IAdminArticleService
{
    private readonly IArticleRepository _articleRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminArticleService> _logger;

    public AdminArticleService(
        IArticleRepository articleRepository,
        ITagRepository tagRepository,
        IUserRepository userRepository,
        ILogger<AdminArticleService> logger)
    {
        _articleRepository = articleRepository;
        _tagRepository = tagRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<PagedResultDto<ArticleListAdminDto>> GetArticlesAsync(
        int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var articles = await _articleRepository.GetPagedArticlesAsync(
            page, pageSize, searchTerm, null, cancellationToken);

        var items = articles.Select(a => new ArticleListAdminDto
        {
            Id = a.Id,
            Title = a.Title,
            Slug = a.Slug,
            Summary = a.Summary,
            ImageUrl = a.ImageUrl,
            IsPublished = a.IsPublished,
            IsFeatured = a.FeatureStatus, // اگر فیلد IsFeatured در Article وجود ندارد
            //ViewCount = a.,
            CreatedAt = a.CreatedDate,
            PublishedAt = a.PublishedAt,
            AuthorName = a.Author?.FullName ?? "کاربر",
            Tags = a.ArticleTags?.Select(at => at.Tag?.Name ?? string.Empty).ToList() ?? new()
        }).ToList();

        var totalCount = await _articleRepository.CountAsync(
            a => string.IsNullOrEmpty(searchTerm) ||
                 a.Title.Contains(searchTerm) ||
                 (a.Summary != null && a.Summary.Contains(searchTerm)),
            cancellationToken);

        return new PagedResultDto<ArticleListAdminDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
        };
    }

    public async Task<ArticleDetailAdminDto?> GetArticleForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetArticleWithDetailsAsync(id, cancellationToken);
        if (article == null) return null;

        var allTags = await _tagRepository.GetAllAsync(cancellationToken);

        return new ArticleDetailAdminDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Content = article.Content,
            ImageUrl = article.ImageUrl,
            IsPublished = article.IsPublished,
            IsFeatured = false, // اگر فیلد IsFeatured در Article وجود ندارد
            //ViewCount = article.ViewCount,
            CreatedAt = article.CreatedDate,
            PublishedAt = article.PublishedAt,
            UpdatedAt = article.LastModifiedDate,
            AuthorId = article.AuthorId.ToString(),
            AuthorName = article.Author?.FullName ?? "کاربر",
            Tags = article.ArticleTags?.Select(at => at.Tag?.Name ?? string.Empty).ToList() ?? new(),
            AllTags = allTags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Slug = t.Slug }).ToList()
        };
    }

    public async Task<AuthResultDto> CreateArticleAsync(CreateArticleAdminDto model,string rootPath, Guid authorId, CancellationToken cancellationToken = default)
    {
        try
        {
            // اعتبارسنجی Slug
            var slug = GenerateSlug(model.Title);
            if (await _articleRepository.ExistsAsync(a => a.Slug == slug, cancellationToken))
            {
				slug = $"{slug}-{Guid.NewGuid().ToString()[8..16]}";
			}

            // آپلود تصویر
            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                imageUrl = await SaveImageAsync(model.ImageFile,rootPath, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                imageUrl = model.ImageUrl;
            }

            var article = new Article
            {
                Title = model.Title,
                Slug = slug,
                Summary = model.Summary ?? string.Empty,
                Content = model.Content,
                ImageUrl = imageUrl,
                IsPublished = model.IsPublished,
                PublishedAt = model.IsPublished ? DateTime.Now : DateTime.Now,
                AuthorId = authorId,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            // پردازش تگ‌ها
            var tags = new List<Tag>();
            foreach (var tagName in model.TagNames.Distinct())
            {
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                var existingTag = await _tagRepository.GetSingleAsync(
                    t => t.Name == tagName, cancellationToken);

                if (existingTag != null)
                {
                    tags.Add(existingTag);
                }
                else
                {
                    var newTag = new Tag
                    {
                        Name = tagName.Trim(),
                        Slug = GenerateSlug(tagName),
                        CreatedDate = DateTime.Now
                    };
                    await _tagRepository.AddAsync(newTag, cancellationToken);
                    tags.Add(newTag);
                }
            }

            article.ArticleTags = tags.Select(t => new ArticleTag
            {
                TagId = t.Id,
                ArticleId = article.Id
            }).ToList();

            await _articleRepository.AddAsync(article, cancellationToken);

            return new AuthResultDto
            {
                Succeeded = true,
                Message = "مقاله با موفقیت ایجاد شد",
                Data = article.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در ایجاد مقاله",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResultDto> UpdateArticleAsync(UpdateArticleAdminDto model, string rootPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var article = await _articleRepository.GetArticleWithDetailsAsync(model.Id, cancellationToken);
            if (article == null)
                return new AuthResultDto { Succeeded = false, Message = "مقاله یافت نشد" };

            // بررسی Slug
            if (article.Slug != model.Slug)
            {
                if (await _articleRepository.ExistsAsync(a => a.Slug == model.Slug && a.Id != model.Id, cancellationToken))
                {
                    return new AuthResultDto { Succeeded = false, Message = "این Slug قبلاً استفاده شده است" };
                }
            }

            // آپلود تصویر جدید
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // حذف تصویر قدیمی
                if (!string.IsNullOrEmpty(article.ImageUrl))
                {
                    DeleteImage(article.ImageUrl,rootPath);
                }
                article.ImageUrl = await SaveImageAsync(model.ImageFile,rootPath, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                article.ImageUrl = model.ImageUrl;
            }

            article.Title = model.Title;
            article.Slug = model.Slug;
            article.Summary = model.Summary ?? string.Empty;
            article.Content = model.Content;
            article.LastModifiedDate = DateTime.Now;
            article.FeatureStatus = model.IsFeatured;
            // تغییر وضعیت انتشار
            if (model.IsPublished != article.IsPublished)
            {
                article.IsPublished = model.IsPublished;
                article.PublishedAt = model.IsPublished ? DateTime.Now : DateTime.Now;
            }

            // به‌روزرسانی تگ‌ها
            var newTagNames = model.TagNames.Distinct().ToList();
            var existingTagNames = article.ArticleTags?.Select(at => at.Tag?.Name ?? string.Empty).ToList() ?? new();

            // حذف تگ‌هایی که دیگر وجود ندارند
            var tagsToRemove = article.ArticleTags?
                .Where(at => !newTagNames.Contains(at.Tag?.Name ?? string.Empty))
                .ToList() ?? new();

            foreach (var tagToRemove in tagsToRemove)
            {
                article.ArticleTags?.Remove(tagToRemove);
            }

            // اضافه کردن تگ‌های جدید
            foreach (var tagName in newTagNames)
            {
                if (existingTagNames.Contains(tagName)) continue;
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                var existingTag = await _tagRepository.GetSingleAsync(
                    t => t.Name == tagName, cancellationToken);

                if (existingTag != null)
                {
                    article.ArticleTags ??= new List<ArticleTag>();
                    article.ArticleTags.Add(new ArticleTag
                    {
                        TagId = existingTag.Id,
                        ArticleId = article.Id
                    });
                }
                else
                {
                    var newTag = new Tag
                    {
                        Name = tagName.Trim(),
                        Slug = GenerateSlug(tagName),
                        CreatedDate = DateTime.Now
                    };
                    await _tagRepository.AddAsync(newTag, cancellationToken);

                    article.ArticleTags ??= new List<ArticleTag>();
                    article.ArticleTags.Add(new ArticleTag
                    {
                        TagId = newTag.Id,
                        ArticleId = article.Id
                    });
                }
            }

            await _articleRepository.UpdateAsync(article, cancellationToken);

            return new AuthResultDto
            {
                Succeeded = true,
                Message = "مقاله با موفقیت بروزرسانی شد"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", model.Id);
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در بروزرسانی مقاله",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResultDto> DeleteArticleAsync(Guid id, string rootPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
            if (article == null)
                return new AuthResultDto { Succeeded = false, Message = "مقاله یافت نشد" };

            // حذف تصویر
            if (!string.IsNullOrEmpty(article.ImageUrl))
            {
                DeleteImage(article.ImageUrl,rootPath);
            }

            await _articleRepository.DeleteAsync(article, cancellationToken);
            return new AuthResultDto { Succeeded = true, Message = "مقاله با موفقیت حذف شد" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در حذف مقاله",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResultDto> TogglePublishStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
            if (article == null)
                return new AuthResultDto { Succeeded = false, Message = "مقاله یافت نشد" };

            article.IsPublished = !article.IsPublished;
            article.PublishedAt = article.IsPublished ? DateTime.Now : DateTime.Now;
            article.LastModifiedDate = DateTime.Now;

            await _articleRepository.UpdateAsync(article, cancellationToken);

            return new AuthResultDto
            {
                Succeeded = true,
                Message = $"مقاله {(article.IsPublished ? "منتشر" : "غیرفعال")} شد"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling article publish status {ArticleId}", id);
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در تغییر وضعیت انتشار",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResultDto> ToggleFeatureStatusAsync(Guid id, bool isFeatured, CancellationToken cancellationToken = default)
    {
        try
        {
            var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
            if (article == null)
                return new AuthResultDto { Succeeded = false, Message = "مقاله یافت نشد" };

            article.FeatureStatus = isFeatured;
			await _articleRepository.UpdateAsync(article, cancellationToken);


			return new AuthResultDto
            {
                Succeeded = true,
                Message = $"مقاله {(isFeatured ? "به منتخب‌ها اضافه" : "از منتخب‌ها حذف")} شد"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling article feature status {ArticleId}", id);
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در تغییر وضعیت منتخب",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<List<TagListDto>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);
        return tags.Select(t => new TagListDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            ArticleCount = t.ArticleTags?.Count ?? 0
        }).ToList();
    }

    public async Task<AuthResultDto> CreateTagAsync(CreateTagDto model, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _tagRepository.ExistsAsync(t => t.Name == model.Name, cancellationToken))
                return new AuthResultDto { Succeeded = false, Message = "این تگ قبلاً وجود دارد" };

            var tag = new Tag
            {
                Name = model.Name.Trim(),
                Slug = GenerateSlug(model.Name),
                CreatedDate = DateTime.Now
            };

            await _tagRepository.AddAsync(tag, cancellationToken);
            return new AuthResultDto
            {
                Succeeded = true,
                Message = "تگ با موفقیت ایجاد شد",
                Data = tag.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag");
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در ایجاد تگ",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResultDto> UpdateTagAsync(UpdateTagDto model, CancellationToken cancellationToken = default)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(model.Id, cancellationToken);
            if (tag == null)
                return new AuthResultDto { Succeeded = false, Message = "تگ یافت نشد" };

            if (await _tagRepository.ExistsAsync(t => t.Name == model.Name && t.Id != model.Id, cancellationToken))
                return new AuthResultDto { Succeeded = false, Message = "این تگ قبلاً وجود دارد" };

            tag.Name = model.Name.Trim();
            tag.Slug = GenerateSlug(model.Name);
            tag.LastModifiedDate = DateTime.Now;

            await _tagRepository.UpdateAsync(tag, cancellationToken);
            return new AuthResultDto { Succeeded = true, Message = "تگ با موفقیت بروزرسانی شد" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagId}", model.Id);
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در بروزرسانی تگ",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResultDto> DeleteTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
            if (tag == null)
                return new AuthResultDto { Succeeded = false, Message = "تگ یافت نشد" };

            await _tagRepository.DeleteAsync(tag, cancellationToken);
            return new AuthResultDto { Succeeded = true, Message = "تگ با موفقیت حذف شد" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId}", id);
            return new AuthResultDto
            {
                Succeeded = false,
                Message = "خطا در حذف تگ",
                Errors = new List<string> { ex.Message }
            };
        }
    }

	#region Helper Methods

	private string GenerateSlug(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return $"article-{Guid.NewGuid():N}".Substring(0, 20);

		var slug = text.Trim().ToLowerInvariant();

		// تبدیل حروف عربی به فارسی
		slug = slug.Replace("ي", "ی").Replace("ك", "ک").Replace("ة", "ه");

		// حذف کاراکترهای غیرمجاز (نگه‌داشتن حروف فارسی، انگلیسی، اعداد، خط تیره)
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF\s-]", "");

		// فاصله‌ها → خط تیره
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

		// خط تیره‌های تکراری → یکی
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-{2,}", "-");

		// حذف خط تیره ابتدا و انتها
		slug = slug.Trim('-');

		// اگر بعد از همه این‌ها خالی شد، از GUID استفاده کن
		if (string.IsNullOrWhiteSpace(slug))
			slug = $"article-{Guid.NewGuid():N}".Substring(0, 20);

		return slug;
	}

	private async Task<string> SaveImageAsync(IFormFile file,string path, CancellationToken cancellationToken = default)
    {
        var uploadsFolder = Path.Combine(path, "uploads", "articles");

        // ایجاد پوشه اگر وجود ندارد
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // ایجاد نام فایل منحصر‌به‌فرد
        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"/uploads/articles/{fileName}";
    }

    private void DeleteImage(string imageUrl,string path)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(path, "uploads", "articles", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting image: {ImageUrl}", imageUrl);
        }
    }

    #endregion
}