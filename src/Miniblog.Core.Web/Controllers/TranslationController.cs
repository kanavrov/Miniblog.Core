using Microsoft.AspNetCore.Mvc;
using Miniblog.Core.Framework.Data;
using Miniblog.Core.Framework.Localization;
using Miniblog.Core.Web.Cache;

namespace Miniblog.Core.Web.Controllers {
	public class TranslationController : Controller {
		private readonly ITranslationProvider _translations;
		
		public TranslationController (ITranslationProvider translations) 
		{
			_translations = translations;

		}

		[Route ("/translations.json")]
		[DisableableOutputCache (Profile = "default")]
		[NoTransaction]
		[HttpGet]
		public IActionResult GetTranslations (string prefix) 
		{
			return Json(_translations.GetAllTranslations(prefix));
		}
	}
}