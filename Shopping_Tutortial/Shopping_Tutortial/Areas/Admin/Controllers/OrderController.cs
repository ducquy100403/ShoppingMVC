using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutortial.Repository;

namespace Shopping_Tutortial.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class OrderController : Controller
	{
		private readonly DataContext _dataContext;

		public OrderController(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Orders.OrderByDescending(o => o.Id).ToListAsync());
		}

		public async Task<IActionResult> ViewOrder(string ordecode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od=>od.Product).Where(od=>od.OrderCode == ordecode).ToListAsync();
			return View(DetailsOrder);
		}
	}
}
