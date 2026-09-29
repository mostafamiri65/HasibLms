using HasibLms.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Presentation.Controllers.Api;

[Authorize(Roles = "Admin")]
[Route("api/upload")]
[ApiController]
public class UploadController : ControllerBase
{
	private readonly IWebHostEnvironment _env;
	private readonly ILogger<UploadController> _logger;

	// فقط این فرمت‌ها مجاز
	private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
	private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
	private const long MaxFileSize = 2 * 1024 * 1024; // ۲ مگابایت

	public UploadController(IWebHostEnvironment env, ILogger<UploadController> logger)
	{
		_env = env;
		_logger = logger;
	}

	[HttpPost("course-image")]
	[RequestSizeLimit(2 * 1024 * 1024)]
	public async Task<IActionResult> UploadCourseImage(IFormFile image)
	{
		if (image == null || image.Length == 0)
			return Ok(new { success = false, message = "فایلی انتخاب نشده است" });

		// بررسی حجم
		if (image.Length > MaxFileSize)
			return Ok(new { success = false, message = "حجم فایل نباید بیشتر از ۲ مگابایت باشد" });

		// بررسی پسوند
		var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
		if (!AllowedExtensions.Contains(ext))
			return Ok(new { success = false, message = "فرمت فایل مجاز نیست. فقط JPG, PNG, WEBP, GIF" });

		// بررسی MIME Type
		if (!AllowedMimeTypes.Contains(image.ContentType.ToLowerInvariant()))
			return Ok(new { success = false, message = "نوع فایل معتبر نیست" });

		// بررسی Magic Number (جلوگیری از جعل پسوند)
		if (!await IsValidImageAsync(image))
			return Ok(new { success = false, message = "فایل انتخاب شده یک تصویر معتبر نیست" });

		try
		{
			// ذخیره
			var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "courses");
			Directory.CreateDirectory(uploadsFolder);

			var fileName = $"{Guid.NewGuid():N}{ext}";
			var filePath = Path.Combine(uploadsFolder, fileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await image.CopyToAsync(stream);
			}

			var url = $"/uploads/courses/{fileName}";
			return Ok(new { success = true, url = url });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error uploading course image");
			return Ok(new { success = false, message = "خطا در آپلود تصویر" });
		}
	}

	private static async Task<bool> IsValidImageAsync(IFormFile file)
	{
		try
		{
			using var stream = file.OpenReadStream();
			var header = new byte[8];
			await stream.ReadAsync(header, 0, 8);

			// JPEG: FF D8 FF
			if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
			// PNG: 89 50 4E 47
			if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
			// GIF: 47 49 46 38
			if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38) return true;
			// WEBP: RIFF....WEBP
			if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46) return true;

			return false;
		}
		catch { return false; }
	}
}