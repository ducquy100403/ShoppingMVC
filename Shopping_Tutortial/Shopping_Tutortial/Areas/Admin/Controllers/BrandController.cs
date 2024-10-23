using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutortial.Models;
using Shopping_Tutortial.Repository;

namespace Shopping_Tutortial.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Brand")]
	[Authorize(Roles = "Admin,Publisher,Author")]
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;
		public BrandController(DataContext context)
		{
			_dataContext = context;
		}

        // Hiển thị danh sách thương hiệu
        [HttpGet]
        [Route("Index")]  // Route sẽ là Admin/Brand/Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10; // Kích thước trang (số lượng bản ghi trên mỗi trang)
            if (pg < 1)
            {
                pg = 1;
            }

            // Đếm tổng số thương hiệu
            int recsCount = await _dataContext.Brands.CountAsync();

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính số lượng bản ghi cần bỏ qua
            var recSkip = (pg - 1) * pageSize;

            // Lấy danh sách thương hiệu có phân trang
            var data = await _dataContext.Brands
                .OrderByDescending(b => b.Id)
                .Skip(recSkip)
                .Take(pager.PageSize)
                .ToListAsync();

            // Truyền thông tin phân trang vào ViewBag để dùng trong view
            ViewBag.Pager = pager;

            // Trả về danh sách thương hiệu đã phân trang cho view
            return View(data);
        }


        // Hiển thị form tạo mới thương hiệu
        [HttpGet]
		[Route("Create")]  // Route sẽ là Admin/Brand/Create
		public async Task<IActionResult> Create()
		{
			return View();
		}

		// Xử lý POST tạo mới thương hiệu
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Create")]  // Route sẽ là Admin/Brand/Create
		public async Task<IActionResult> Create(BrandModel brand)
		{
			if (ModelState.IsValid)
			{
				// Xử lý thêm mới thương hiệu
				brand.Slug = brand.Name.Replace(" ", "-");
				var slug = await _dataContext.Brands.FirstOrDefaultAsync(b => b.Slug == brand.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Thương hiệu đã có trong cơ sở dữ liệu");
					return View(brand);
				}

				_dataContext.Add(brand);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm thương hiệu thành công";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Model có một vài lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			return View(brand);
		}

		// Hiển thị form chỉnh sửa thương hiệu
		[HttpGet]
		[Route("Edit/{id}")]  // Route sẽ là Admin/Brand/Edit/{id}
		public async Task<IActionResult> Edit(int id)
		{
			BrandModel brand = await _dataContext.Brands.FindAsync(id);
			if (brand == null)
			{
				return NotFound();
			}
			return View(brand);
		}

		// Xử lý POST cập nhật thương hiệu
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Edit/{id}")]  // Route sẽ là Admin/Brand/Edit/{id}
		public async Task<IActionResult> Edit(BrandModel brand)
		{
			if (ModelState.IsValid)
			{
				// Xử lý cập nhật dữ liệu
				brand.Slug = brand.Name.Replace(" ", "-");
				var slug = await _dataContext.Brands.FirstOrDefaultAsync(c => c.Slug == brand.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Thương hiệu đã có trong cơ sở dữ liệu");
					return View(brand);
				}

				_dataContext.Update(brand);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thương hiệu thành công";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Model có một vài lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			return View(brand);
		}

		// Xóa thương hiệu
		[HttpGet]
		[Route("Delete/{id}")]  // Route sẽ là Admin/Brand/Delete/{id}
		public async Task<IActionResult> Delete(int id)
		{
			BrandModel brand = await _dataContext.Brands.FindAsync(id);
			if (brand == null)
			{
				return NotFound();
			}

			_dataContext.Brands.Remove(brand);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Thương hiệu đã bị xoá";
			return RedirectToAction("Index");
		}
	}
}
