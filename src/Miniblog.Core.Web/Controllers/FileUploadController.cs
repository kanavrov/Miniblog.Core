using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Miniblog.Core.Framework.Data;
using Miniblog.Core.Service.Exceptions;
using Miniblog.Core.Service.Persistence;

namespace Miniblog.Core.Web.Controllers
{
	public class FileUploadController : Controller {
		private readonly IFilePersisterService _service;

		public FileUploadController(IFilePersisterService service)
		{
			_service = service;
		}

		[HttpPost]
		[NoTransaction]
		public async Task<IActionResult> Image(IFormFile image) 
		{
			try 
			{
				return Ok(await _service.SaveImage(image.OpenReadStream(), image.FileName, image.ContentType));
			}
			catch(Exception)
			{
				return BadRequest();
			}
		}
	}
}
