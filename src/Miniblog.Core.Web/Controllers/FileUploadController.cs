using System;
using Microsoft.AspNetCore.Mvc;

namespace Miniblog.Core.Web.Controllers
{
	public class FileUploadController : Controller {
		
		[HttpPost]
		public string Image() {
			return "localhost";
		}
	}
}
