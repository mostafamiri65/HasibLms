using HasibLms.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HasibLms.Persentation.Controllers;

public class HomeController : Controller
{
	private readonly ICourseService _courseService;
	private readonly IArticleService _articleService;
	private readonly ISiteService _siteService;
	private readonly ILogger<HomeController> _logger;

	public HomeController(
		ICourseService courseService,
		IArticleService articleService,
		ISiteService siteService,
		ILogger<HomeController> logger)
	{
		_courseService = courseService;
		_articleService = articleService;
		_siteService = siteService;
		_logger = logger;
	}

	[ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page" })]
	public async Task<IActionResult> Index(CancellationToken cancellationToken)
	{
		try
		{
			var settings = await _siteService.GetSettingsAsync(cancellationToken);
			ViewBag.Title = settings?.InstituteName ?? "صفحه اصلی";
			ViewBag.MetaDescription = settings?.MetaDescription ?? "بزرگترین پلتفرم آموزش تخصصی حسابداری، مالیات و سرمایه‌گذاری در ایران";

			var model = await _courseService.GetHomePageDataAsync(cancellationToken);
			return View(model);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading home page");
			return View("Error");
		}
	}

	[ResponseCache(Duration = 3600)]
	public async Task<IActionResult> About(CancellationToken cancellationToken)
	{
		try
		{
			var settings = await _siteService.GetSettingsAsync(cancellationToken);
			ViewBag.Title = "درباره ما";
			ViewBag.MetaDescription = settings?.AboutText?.Length > 150
				? settings.AboutText[..150]
				: settings?.AboutText ?? "درباره موسسه آموزشی مالیبا";

			return View(settings);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading about page");
			return View("Error");
		}
	}

	[ResponseCache(Duration = 3600)]
	public async Task<IActionResult> Contact(CancellationToken cancellationToken)
	{
		try
		{
			var settings = await _siteService.GetSettingsAsync(cancellationToken);
			ViewBag.Title = "تماس با ما";
			return View(settings);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading contact page");
			return View("Error");
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Contact(string name, string email, string phone, string message, CancellationToken cancellationToken)
	{
		try
		{
			if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
			{
				TempData["Error"] = "لطفاً تمام فیلدهای الزامی را پر کنید";
				return RedirectToAction("Contact");
			}

			// TODO: Send email via service
			// await _emailService.SendContactEmailAsync(name, email, phone, message, cancellationToken);

			TempData["Success"] = "پیام شما با موفقیت ارسال شد. به زودی با شما تماس می‌گیریم.";
			_logger.LogInformation("Contact message received from {Name} ({Email})", name, email);

			return RedirectToAction("Contact");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sending contact message");
			TempData["Error"] = "خطا در ارسال پیام. لطفاً مجدداً تلاش کنید.";
			return RedirectToAction("Contact");
		}
	}

	[ResponseCache(Duration = 86400)]
	public IActionResult Privacy()
	{
		ViewBag.Title = "حریم خصوصی";
		return View();
	}

	[ResponseCache(Duration = 86400)]
	public IActionResult Terms()
	{
		ViewBag.Title = "قوانین و مقررات";
		return View();
	}

	[ResponseCache(Duration = 86400)]
	public IActionResult FAQ()
	{
		ViewBag.Title = "سوالات متداول";
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Subscribe(string email, CancellationToken cancellationToken)
	{
		try
		{
			if (string.IsNullOrEmpty(email) || !email.Contains("@"))
			{
				return Json(new { success = false, message = "ایمیل معتبر وارد کنید" });
			}

			// TODO: Save to newsletter via service
			// await _newsletterService.SubscribeAsync(email, cancellationToken);

			return Json(new { success = true, message = "عضویت با موفقیت انجام شد" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error subscribing email {Email}", email);
			return Json(new { success = false, message = "خطا در عضویت" });
		}
	}
}