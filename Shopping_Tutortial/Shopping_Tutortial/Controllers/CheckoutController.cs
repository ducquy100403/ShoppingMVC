using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shopping_Tutortial.Models;
using Shopping_Tutortial.Models.ViewModels;
using Shopping_Tutortial.Repository;
using System.Security.Claims;

namespace Shopping_Tutortial.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		public CheckoutController(DataContext context)
		{
			_dataContext = context;
		}

		public async Task<IActionResult> Checkout( )
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var odercode = Guid.NewGuid().ToString();//123
				var oderItem = new OrderModel();
				oderItem.OrderCode = odercode;
				oderItem.UserName = userEmail;
				oderItem.Status = 1;
				oderItem.CreatedDate = DateTime.Now;
				_dataContext.Add(oderItem);
				_dataContext.SaveChanges();
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach (var cart in cartItems)
				{
					var orderdetails = new OrderDetails();
					orderdetails.UserName = userEmail;
					orderdetails.OrderCode = odercode;
					orderdetails.ProductId = cart.ProductId;
					orderdetails.Price = cart.Price;
					orderdetails.Quantity = cart.Quantity;
					_dataContext.Add(orderdetails);
					_dataContext.SaveChanges();

				}
				HttpContext.Session.Remove("Cart");
				TempData["success"] = "Checkout thanh cong, vui long cho duyet don hang";
				return RedirectToAction("Index", "Cart");

			}
			return View();
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
