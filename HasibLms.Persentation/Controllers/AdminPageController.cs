//using HasibLms.Domain.Interfaces;
//using HasibLms.Shared.DTOs.Page;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace HasibLms.Persentation.Controllers
//{
//	[Authorize(Roles = "Admin")]
//	[Route("admin/pages")]
//	public class AdminPageController : Controller
//	{
//		private readonly IPageService _pageService;

//		// لیست صفحات
//		[HttpGet]
//		public async Task<IActionResult> Index(int page = 1)

//	// ایجاد صفحه جدید
//	[HttpGet("create")]
//		public IActionResult Create()


//	[HttpPost("create")]
//		public async Task<IActionResult> Create(CreatePageDto model)

//	// ویرایش صفحه (با سازنده صفحه)
//	[HttpGet("edit/{id}")]
//		public async Task<IActionResult> Edit(Guid id)


//	[HttpPost("edit")]
//		public async Task<IActionResult> Edit(UpdatePageDto model)

//	// مدیریت بلوک‌ها
//	[HttpPost("block/add")]
//		public async Task<IActionResult> AddBlock(Guid pageId, CreatePageBlockDto model)


//	[HttpPost("block/update")]
//		public async Task<IActionResult> UpdateBlock(UpdatePageBlockDto model)


//	[HttpPost("block/delete/{id}")]
//		public async Task<IActionResult> DeleteBlock(Guid id)


//	[HttpPost("block/reorder")]
//		public async Task<IActionResult> ReorderBlocks(Guid pageId, [FromBody] List<Guid> blockIds)
//}
//}
