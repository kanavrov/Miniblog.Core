using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Miniblog.Core.Service.Blog;
using Miniblog.Core.Service.Models;

namespace Miniblog.Core.Web.Controllers
{
	[Authorize]	
	[NoClientCache]
	public class CategoryController : Controller
	{
		private readonly IBlogService _blog;

		public CategoryController(IBlogService blog)
		{
			_blog = blog;
		}

		[Route("/category")]
		public async Task<IActionResult> Index()
		{
			var categories = await _blog.GetCategories();
			return View(categories);
		}

		[Route("/category/edit/{id?}")]
		[HttpGet]
		public async Task<IActionResult> Edit(Guid id)
		{
			if (id == Guid.Empty)
			{
				return View(new CategoryDto());
			}

			var category = await _blog.GetCategoryById(id);

			if (category != null)
			{
				return View(category);
			}

			return NotFound();
		}

		[Route("/category/update")]
		[HttpPost, AutoValidateAntiforgeryToken]
		public async Task<IActionResult> Update(CategoryDto category)
		{
			if (!ModelState.IsValid)
			{
				return View("Edit", category);
			}

			await _blog.UpdateCategory(category);

			return RedirectToAction(nameof(Index));
		}

		[Route("/category/add")]
		[HttpPost, AutoValidateAntiforgeryToken]
		public async Task<IActionResult> Add(CategoryDto category)
		{
			if (!ModelState.IsValid)
			{
				return View("Edit", category);
			}

			await _blog.AddCategory(category);

			return RedirectToAction(nameof(Index));
		}

		[Route("/category/delete/{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var existing = await _blog.GetCategoryById(id);

			if (existing != null)
			{
				await _blog.DeleteCategory(id);
				return RedirectToAction(nameof(Index));
			}

			return NotFound();
		}
	}


}
