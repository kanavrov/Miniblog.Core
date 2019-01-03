using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Miniblog.Core.Models;
using Miniblog.Core.Services;
using WebEssentials.AspNetCore.Pwa;

namespace Miniblog.Core.Controllers
{
	public class BlogController : Controller
	{
		private readonly IBlogService _blog;
		private readonly IOptionsSnapshot<BlogSettings> _settings;
		private readonly WebManifest _manifest;
		private readonly IRouteService _routeService;

		public BlogController(IBlogService blog, IOptionsSnapshot<BlogSettings> settings,
		WebManifest manifest, IRouteService routeService)
		{
			_routeService = routeService;
			_blog = blog;
			_settings = settings;
			_manifest = manifest;
		}

		[Route("/{page:int?}")]
		[OutputCache(Profile = "default")]
		public async Task<IActionResult> Index([FromRoute]int page = 0)
		{
			var posts = await _blog.GetPosts(_settings.Value.PostsPerPage, _settings.Value.PostsPerPage * page);
			ViewData["Title"] = _manifest.Name;
			ViewData["Description"] = _manifest.Description;
			ViewData["prev"] = $"/{page + 1}/";
			ViewData["next"] = $"/{(page <= 1 ? null : page - 1 + "/")}";
			return View("~/Views/Blog/Index.cshtml", posts);
		}

		[Route("/blog/category/{category}/{page:int?}")]
		[OutputCache(Profile = "default")]
		public async Task<IActionResult> Category(string category, int page = 0)
		{
			var posts = (await _blog.GetPostsByCategory(category)).Skip(_settings.Value.PostsPerPage * page).Take(_settings.Value.PostsPerPage);
			ViewData["Title"] = _manifest.Name + " " + category;
			ViewData["Description"] = $"Articles posted in the {category} category";
			ViewData["prev"] = $"/blog/category/{category}/{page + 1}/";
			ViewData["next"] = $"/blog/category/{category}/{(page <= 1 ? null : page - 1 + "/")}";
			return View("~/Views/Blog/Index.cshtml", posts);
		}

		// This is for redirecting potential existing URLs from the old Miniblog URL format
		[Route("/post/{slug}")]
		[HttpGet]
		public IActionResult Redirects(string slug)
		{
			return LocalRedirectPermanent($"/blog/{slug}");
		}

		[Route("/blog/{slug?}")]
		[OutputCache(Profile = "default")]
		public async Task<IActionResult> Post(string slug)
		{
			var post = await _blog.GetPostBySlug(slug);

			if (post != null)
			{
				return View(post);
			}

			return NotFound();
		}

		[Route("/blog/edit/{id?}")]
		[HttpGet, Authorize]
		public async Task<IActionResult> Edit(string id)
		{
			ViewData["AllCats"] = (await _blog.GetCategories()).ToList();

			if (string.IsNullOrEmpty(id))
			{
				return View(new Post());
			}

			var post = await _blog.GetPostById(id);

			if (post != null)
			{
				return View(post);
			}

			return NotFound();
		}

		[Route("/blog/{slug?}")]
		[HttpPost, Authorize, AutoValidateAntiforgeryToken]
		public async Task<IActionResult> UpdatePost(Post post)
		{
			if (!ModelState.IsValid)
			{
				return View("Edit", post);
			}

			await _blog.SavePost(post, Request.Form["categories"]);

			return Redirect(_routeService.GetEncodedLink(post));
		}

		[Route("/blog/deletepost/{id}")]
		[HttpPost, Authorize, AutoValidateAntiforgeryToken]
		public async Task<IActionResult> DeletePost(string id)
		{
			var existing = await _blog.GetPostById(id);

			if (existing != null)
			{
				await _blog.DeletePost(id);
				return Redirect("/");
			}

			return NotFound();
		}

		[Route("/blog/comment/{postId}")]
		[HttpPost]
		public async Task<IActionResult> AddComment(string postId, Comment comment)
		{
			// the website form key should have been removed by javascript
			// unless the comment was posted by a spam robot
			if (!Request.Form.ContainsKey("website"))
			{
				return BadRequest();
			}

			var post = await _blog.GetPostById(postId);

			if (!ModelState.IsValid)
			{
				return View("Post", post);
			}

			if (post == null)
			{
				return NotFound();
			}

			await _blog.AddComment(comment, postId);

			return Redirect(_routeService.GetCommentLink(post, comment));
		}

		[Route("/blog/comment/{postId}/{commentId}")]
		[Authorize]
		public async Task<IActionResult> DeleteComment(string postId, string commentId)
		{
			var post = await _blog.GetPostById(postId);

			if (post == null)
			{
				return NotFound();
			}

			await _blog.DeleteComment(commentId, postId);

			return Redirect(_routeService.GetCommentLink(post));
		}
	}
}
