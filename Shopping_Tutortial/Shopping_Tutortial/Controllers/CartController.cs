using Microsoft.AspNetCore.Mvc;
using Shopping_Tutortial.Models;
using Shopping_Tutortial.Models.ViewModels;
using Shopping_Tutortial.Repository;

namespace Shopping_Tutortial.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;

		public CartController(DataContext _context)
		{
			_dataContext = _context;
		}

		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List< CartItemModel >>("Cart") ?? new List<CartItemModel>();
			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity*x.Price),
			};
			return View(cartVM);
		}

		public ActionResult Checkout() 
		{
			return View("~/View/Checkout/Index.cshtml");
		}

		public async Task<IActionResult> Add(int Id) 
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			List<CartItemModel> cart= HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c =>c.ProductId == Id ).FirstOrDefault();

			if(cartItem == null)
			{
				// Nếu chưa có, thêm sản phẩm mới vào giỏ hàng
				cart.Add(new CartItemModel
				{
					ProductId = product.Id,
					ProductName = product.Name,
					Price = product.Price,
					Quantity = 1 // Thêm sản phẩm với số lượng 1
				});
			}

			else
			{
				cartItem.Quantity += 1;
			}
			HttpContext.Session.SetJson("Cart", cart);

			TempData["success"] = "Add item to cart SuccsesFully";
			return Redirect(Request.Headers["Referer"].ToString());
		}

		public async Task<IActionResult> Decrease(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartItem = cart.Where(c=>c.ProductId == Id).FirstOrDefault();

			if (cartItem.Quantity > 1)
			{
				--cartItem.Quantity;
			}
			else
			{
				cart.RemoveAll(p=>p.ProductId == Id);
			}

			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else 
			{
				HttpContext.Session.SetJson("Cart", cart);
			}
			TempData["success"] = "Decrease item to cart SuccsesFully";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Increase(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItem.Quantity >= 1)
			{
				++cartItem.Quantity;
			}
			else
			{
				cart.RemoveAll(p => p.ProductId == Id);
			}

			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
			}
			TempData["success"] = "Increase item to cart SuccsesFully";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Remove(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			cart.RemoveAll(p=>p.ProductId == Id);
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}

			else 
			{
				HttpContext.Session.SetJson("Cart", cart);
			}
			TempData["success"] = "Remove item to cart SuccsesFully";
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Clear(int Id)
		{
			HttpContext.Session.Remove("Cart");
			TempData["success"] = "Clear All Item to cart SuccsesFully";
			return RedirectToAction("Index");
		}
	}
}
