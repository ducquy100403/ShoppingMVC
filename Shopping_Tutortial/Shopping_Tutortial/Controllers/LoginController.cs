using Microsoft.AspNetCore.Mvc;

namespace Shopping_Tutortial.Controllers
{
	public class LoginController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
