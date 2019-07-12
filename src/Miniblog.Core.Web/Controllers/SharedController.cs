using Microsoft.AspNetCore.Mvc;
using Miniblog.Core.Framework.Data;

namespace Miniblog.Core.Web.Controllers
{
	public class SharedController : Controller
	{
		[NoTransaction]
		public IActionResult Error()
		{
			return View(Response.StatusCode);
		}

		public IActionResult Offline()
		{
			return View();
		}

		public IActionResult Pwa()
		{
			return View();
		}
	}
}
