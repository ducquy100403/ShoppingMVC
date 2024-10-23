using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutortial.Models;
using Shopping_Tutortial.Repository;

namespace Shopping_Tutortial.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Category")]
	[Authorize(Roles = "Admin")]
	public class CategoryController : Controller
	{
		private readonly DataContext _dataContext;
		public CategoryController(DataContext context)
		{
			_dataContext = context;
		}

        // Hiển thị danh sách danh mục
        [HttpGet]
        [Route("Index")]  // Định nghĩa route cho Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Không gọi ToList() sớm
            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy tổng số bản ghi trước khi phân trang
            int recsCount = await _dataContext.Categories.CountAsync();

            // Tạo đối tượng Paginate cho việc phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính số bản ghi cần bỏ qua
            var recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu phân trang từ database
            var data = await _dataContext.Categories
                .Skip(recSkip)   // Bỏ qua số lượng bản ghi
                .Take(pager.PageSize)  // Lấy số lượng bản ghi cần thiết
                .ToListAsync();  // Chỉ thực hiện gọi ToListAsync() sau khi áp dụng phân trang

            ViewBag.Pager = pager;

            // Trả về dữ liệu phân trang cho view
            return View(data);
        }


        // Hiển thị form chỉnh sửa danh mục
        [HttpGet]
		[Route("Edit/{id}")]  // Định nghĩa route cho Edit với tham số {id}
		public async Task<IActionResult> Edit(int id)
		{
			CategoryModel category = await _dataContext.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}

        // Xử lý cập nhật danh mục
        [HttpPost]
        [Route("Edit/{id}")]  // Định nghĩa route cho POST Edit với tham số {id}
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                // Tìm danh mục trong cơ sở dữ liệu dựa trên id
                var existingCategory = await _dataContext.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin cần thiết
                existingCategory.Name = category.Name;
                existingCategory.Slug = category.Name.Replace(" ", "-");
                existingCategory.Status = category.Status;  // Cập nhật trạng thái

                // Kiểm tra slug đã tồn tại chưa (loại bỏ slug hiện tại của danh mục này ra khỏi kiểm tra)
                var slug = await _dataContext.Categories
                                .FirstOrDefaultAsync(c => c.Slug == existingCategory.Slug && c.Id != id);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã có trong cơ sở dữ liệu");
                    return View(category);
                }

                // Cập nhật danh mục
                _dataContext.Update(existingCategory);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Model có một vài lỗi";
            return View(category);
        }


        // Hiển thị form tạo danh mục mới
        [HttpGet]
		[Route("Create")]  // Định nghĩa route cho Create
		public IActionResult Create()
		{
			return View();
		}

		// Xử lý thêm mới danh mục
		[HttpPost]
		[Route("Create")]  // Định nghĩa route cho POST Create
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CategoryModel category)
		{
			if (ModelState.IsValid)
			{
				// Kiểm tra slug đã tồn tại chưa
				category.Slug = category.Name.Replace(" ", "-");
				var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Danh mục đã có trong cơ sở dữ liệu");
					return View(category);
				}

				_dataContext.Add(category);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm danh mục thành công";
				return RedirectToAction("Index");
			}

			TempData["error"] = "Model có một vài lỗi";
			return View(category);
		}

		// Xóa danh mục
		[HttpGet]
		[Route("Delete/{id}")]  // Định nghĩa route cho Delete với tham số {id}
		public async Task<IActionResult> Delete(int id)
		{
			CategoryModel category = await _dataContext.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			_dataContext.Categories.Remove(category);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Danh mục đã bị xoá";
			return RedirectToAction("Index");
		}
	}
}
