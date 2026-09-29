using System.Globalization;

namespace HasibLms.Shared.Extensions;

public static class DateTimeExtensions
{
	// نام‌های ماه‌های شمسی
	private static readonly string[] PersianMonths =
	{
		"فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
		"مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
	};

	// نام‌های روزهای هفته شمسی
	private static readonly string[] PersianDaysOfWeek =
	{
		"یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"
	};

	/// <summary>
	/// تبدیل تاریخ میلادی به شمسی با فرمت پیش‌فرض: 1403/05/15 - 14:30
	/// </summary>
	public static string ToPersianDateTime(this DateTime dateTime, string separator = " - ")
	{
		if (dateTime == default) return "-";

		var pc = new PersianCalendar();

		var year = pc.GetYear(dateTime);
		var month = pc.GetMonth(dateTime);
		var day = pc.GetDayOfMonth(dateTime);
		var hour = dateTime.Hour;
		var minute = dateTime.Minute;

		return $"{year:0000}/{month:00}/{day:00}{separator}{hour:00}:{minute:00}";
	}

	/// <summary>
	/// تبدیل تاریخ میلادی به شمسی با فرمت پیش‌فرض و بدون ساعت: 1403/05/15
	/// </summary>
	public static string ToPersianDate(this DateTime dateTime, string separator = "/")
	{
		if (dateTime == default) return "-";

		var pc = new PersianCalendar();

		var year = pc.GetYear(dateTime);
		var month = pc.GetMonth(dateTime);
		var day = pc.GetDayOfMonth(dateTime);

		return $"{year:0000}{separator}{month:00}{separator}{day:00}";
	}

	/// <summary>
	/// تبدیل تاریخ میلادی به شمسی با نام ماه: 15 مرداد 1403
	/// </summary>
	public static string ToPersianDateLong(this DateTime dateTime)
	{
		if (dateTime == default) return "-";

		var pc = new PersianCalendar();

		var year = pc.GetYear(dateTime);
		var month = pc.GetMonth(dateTime);
		var day = pc.GetDayOfMonth(dateTime);

		return $"{day} {PersianMonths[month - 1]} {year}";
	}

	/// <summary>
	/// تبدیل تاریخ میلادی به شمسی با نام روز هفته: شنبه 15 مرداد 1403
	/// </summary>
	public static string ToPersianDateFull(this DateTime? dateTime)
	{
		if (dateTime == null) return "-";
		if (dateTime == default) return "-";

		var pc = new PersianCalendar();
		var date = Convert.ToDateTime(dateTime);
		var year = pc.GetYear(date);
		var month = pc.GetMonth(date);
		var day = pc.GetDayOfMonth(date);
		var dayOfWeek = (int)date.DayOfWeek;

		return $"{PersianDaysOfWeek[dayOfWeek]} {day} {PersianMonths[month - 1]} {year}";
	}
	public static string ToPersianDateFull(this DateTime dateTime)
	{

		if (dateTime == default) return "-";

		var pc = new PersianCalendar();
		var date = Convert.ToDateTime(dateTime);
		var year = pc.GetYear(date);
		var month = pc.GetMonth(date);
		var day = pc.GetDayOfMonth(date);
		var dayOfWeek = (int)date.DayOfWeek;

		return $"{PersianDaysOfWeek[dayOfWeek]} {day} {PersianMonths[month - 1]} {year}";
	}

	/// <summary>
	/// نمایش زمان نسبی: "الان"، "5 دقیقه پیش"، "دیروز"، "3 روز پیش"، "2 هفته پیش"، "3 ماه پیش"
	/// </summary>
	public static string ToPersianRelativeTime(this DateTime dateTime)
	{
		if (dateTime == default) return "-";

		var now = DateTime.Now;
		var diff = now - dateTime;

		if (diff.TotalSeconds < 60)
			return "همین الان";

		if (diff.TotalMinutes < 60)
			return $"{(int)diff.TotalMinutes} دقیقه پیش";

		if (diff.TotalHours < 24)
			return $"{(int)diff.TotalHours} ساعت پیش";

		if (diff.TotalDays < 2)
			return "دیروز";

		if (diff.TotalDays < 7)
			return $"{(int)diff.TotalDays} روز پیش";

		if (diff.TotalDays < 30)
			return $"{(int)(diff.TotalDays / 7)} هفته پیش";

		if (diff.TotalDays < 365)
			return $"{(int)(diff.TotalDays / 30)} ماه پیش";

		return $"{(int)(diff.TotalDays / 365)} سال پیش";
	}

	/// <summary>
	/// تبدیل تاریخ میلادی به شمسی با فرمت دلخواه (yyyy, MM, dd, HH, mm, ss)
	/// </summary>
	public static string ToPersianFormat(this DateTime dateTime, string format)
	{
		if (dateTime == default) return "-";

		var pc = new PersianCalendar();

		return format
			.Replace("yyyy", pc.GetYear(dateTime).ToString("0000"))
			.Replace("yy", (pc.GetYear(dateTime) % 100).ToString("00"))
			.Replace("MM", pc.GetMonth(dateTime).ToString("00"))
			.Replace("dd", pc.GetDayOfMonth(dateTime).ToString("00"))
			.Replace("HH", dateTime.Hour.ToString("00"))
			.Replace("mm", dateTime.Minute.ToString("00"))
			.Replace("ss", dateTime.Second.ToString("00"));
	}

	/// <summary>
	/// تبدیل DateTime? (nullable) به شمسی - اگر null باشد "-" برمی‌گرداند
	/// </summary>
	public static string ToPersianDateTime(this DateTime? dateTime, string separator = " - ")
	{
		return dateTime.HasValue ? dateTime.Value.ToPersianDateTime(separator) : "-";
	}

	/// <summary>
	/// تبدیل DateTime? (nullable) به شمسی بدون ساعت
	/// </summary>
	public static string ToPersianDate(this DateTime? dateTime)
	{
		return dateTime.HasValue ? dateTime.Value.ToPersianDate() : "-";
	}
}