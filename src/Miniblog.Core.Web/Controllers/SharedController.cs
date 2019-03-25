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

		/// <summary>
		///  This is for use in wwwroot/serviceworker.js to support offline scenarios
		/// </summary>
		public IActionResult Offline()
		{
			return View();
		}
	}
}
