using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Miniblog.Core.Framework.Data;
using Miniblog.Core.Service.Blog;
using Miniblog.Core.Service.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Miniblog.Core.Web.Controllers
{
	[Authorize]
	[NoClientCache]
	public class AccountController : Controller
	{
		private readonly IUserService _userServices;

		public AccountController(IUserService userServices)
		{
			_userServices = userServices;
		}

		[Route("/login")]
		[AllowAnonymous]
		[HttpGet]
		[NoTransaction]
		public IActionResult Login(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		[Route("/login")]
		[HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		[NoTransaction]
		public async Task<IActionResult> LoginAsync(string returnUrl, LoginViewModel model)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (ModelState.IsValid && _userServices.ValidateUser(model.UserName, model.Password))
			{
				var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
				identity.AddClaim(new Claim(ClaimTypes.Name, model.UserName));

				var principle = new ClaimsPrincipal(identity);
				var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };
				await HttpContext.SignInAsync(principle, properties);

				return LocalRedirect(returnUrl ?? "/");
			}

			ModelState.AddModelError(string.Empty, "Username or password is invalid.");
			return View("Login", model);
		}

		[Route("/logout")]
		[NoTransaction]
		public async Task<IActionResult> LogOutAsync()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return LocalRedirect("/");
		}
	}
}
