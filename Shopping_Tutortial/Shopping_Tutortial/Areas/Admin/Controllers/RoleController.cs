using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutortial.Repository;

namespace Shopping_Tutortial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Role")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        public RoleController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Roles.OrderByDescending(r => r.Id).ToListAsync());
        }

		// Hiển thị form tạo mới thương hiệu
		[HttpGet]
		[Route("Create")]  // Route sẽ là Admin/Role/Create
		public async Task<IActionResult> Create()
		{
			return View();
		}
	}
}
