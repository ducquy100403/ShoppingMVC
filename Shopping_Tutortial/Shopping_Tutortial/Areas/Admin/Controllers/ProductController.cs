using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutortial.Models;
using Shopping_Tutortial.Repository;

namespace Shopping_Tutortial.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Product")]
	[Authorize(Roles = "Admin")]
	public class ProductController : Controller
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly DataContext _dataContext;

		public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}

        // Hiển thị danh sách sản phẩm
        [HttpGet]
        [Route("Index")]  // Route rõ ràng cho trang Index: /Admin/Product/Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10; // Số sản phẩm trên mỗi trang
            if (pg < 1)
            {
                pg = 1;
            }

            // Đếm tổng số sản phẩm
            int recsCount = await _dataContext.Products.CountAsync();

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính số bản ghi cần bỏ qua
            var recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu sản phẩm có phân trang từ database
            var data = await _dataContext.Products
                .OrderByDescending(p => p.Id)
                .Include(p => p.Category)  // Bao gồm Category
                .Include(p => p.Brand)     // Bao gồm Brand
                .Skip(recSkip)
                .Take(pager.PageSize)
                .ToListAsync();

            // Truyền thông tin phân trang vào ViewBag
            ViewBag.Pager = pager;

            // Trả về view với danh sách sản phẩm đã phân trang
            return View(data);
        }


        // Hiển thị form tạo sản phẩm mới
        [HttpGet]
		[Route("Create")] // Route rõ ràng cho trang tạo mới: /Admin/Product/Create
		public IActionResult Create()
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
			return View();
		}

		// Xử lý thêm mới sản phẩm
		[HttpPost]
		[Route("Create")] // Route rõ ràng cho POST tạo mới: /Admin/Product/Create
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductModel product)
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			if (ModelState.IsValid)
			{
				product.Slug = product.Name.Replace(" ", "-");
				var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Sản phẩm đã có trong cơ sở dữ liệu");
					return View(product);
				}

				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					using (FileStream fs = new FileStream(filePath, FileMode.Create))
					{
						await product.ImageUpload.CopyToAsync(fs);
					}

					product.Image = imageName;
				}

				_dataContext.Add(product);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm sản phẩm thành công";
				return RedirectToAction("Index");
			}

			TempData["error"] = "Có lỗi trong quá trình thêm sản phẩm";
			return View(product);
		}

		// Hiển thị form chỉnh sửa sản phẩm
		[HttpGet]
		[Route("Edit/{id}")] // Route rõ ràng cho trang chỉnh sửa: /Admin/Product/Edit/{id}
		public async Task<IActionResult> Edit(int id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}

			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
			return View(product);
		}

		// Xử lý cập nhật sản phẩm
		[HttpPost]
		[Route("Edit/{id}")] // Route rõ ràng cho POST chỉnh sửa: /Admin/Product/Edit/{id}
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ProductModel product)
		{
			var existed_product = await _dataContext.Products.FindAsync(product.Id);
			if (existed_product == null)
			{
				return NotFound();
			}

			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			if (ModelState.IsValid)
			{
				product.Slug = product.Name.Replace(" ", "-");

				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					if (!string.IsNullOrEmpty(existed_product.Image))
					{
						string oldImage = Path.Combine(uploadsDir, existed_product.Image);
						if (System.IO.File.Exists(oldImage))
						{
							try
							{
								System.IO.File.Delete(oldImage);
							}
							catch (Exception ex)
							{
								ModelState.AddModelError("", "Có lỗi xảy ra khi xóa hình ảnh cũ");
								return View(product);
							}
						}
					}

					using (FileStream fs = new FileStream(filePath, FileMode.Create))
					{
						await product.ImageUpload.CopyToAsync(fs);
					}

					existed_product.Image = imageName;
				}

				existed_product.Name = product.Name;
				existed_product.Description = product.Description;
				existed_product.Price = product.Price;
				existed_product.CategoryId = product.CategoryId;
				existed_product.BrandId = product.BrandId;

				_dataContext.Update(existed_product);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật sản phẩm thành công";
				return RedirectToAction("Index");
			}

			TempData["error"] = "Có lỗi trong quá trình cập nhật sản phẩm";
			return View(product);
		}

		// Xóa sản phẩm
		[HttpGet]
		[Route("Delete/{id}")] // Route rõ ràng cho trang xóa: /Admin/Product/Delete/{id}
		public async Task<IActionResult> Delete(int id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}

			string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
			if (!string.IsNullOrEmpty(product.Image))
			{
				string oldImage = Path.Combine(uploadsDir, product.Image);
				if (System.IO.File.Exists(oldImage))
				{
					try
					{
						System.IO.File.Delete(oldImage);
					}
					catch (Exception ex)
					{
						ModelState.AddModelError("", "Có lỗi xảy ra khi xóa hình ảnh");
						return View(product);
					}
				}
			}

			_dataContext.Products.Remove(product);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Sản phẩm đã bị xoá";
			return RedirectToAction("Index");
		}
	}
}
