using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Miniblog.Core.Contract.Models;
using Miniblog.Core.Data.Repositories;
using Miniblog.Core.Service.Models;
using WilderMinds.MetaWeblog;

namespace Miniblog.Core.Service.Services
{
	public class MetaWeblogService : IMetaWeblogProvider
	{
		private readonly IBlogRepository _blog;
		private readonly IConfiguration _config;
		private readonly IUserService _userServices;
		private readonly IFilePersisterService _filePersisterService;
		private readonly IHttpContextAccessor _context;
		private readonly IRouteService _routeService;

		public MetaWeblogService(IBlogRepository blog, IConfiguration config, IHttpContextAccessor context,
		IUserService userServices, IFilePersisterService filePersisterService, IRouteService routeService)
		{
			_routeService = routeService;
			_blog = blog;
			_config = config;
			_userServices = userServices;
			_filePersisterService = filePersisterService;
			_context = context;
		}

		public async Task<string> AddPostAsync(string blogid, string username, string password, WilderMinds.MetaWeblog.Post post, bool publish)
		{
			ValidateUser(username, password);

			var newPost = new PostDto
			{
				Title = post.title,
				Slug = !string.IsNullOrWhiteSpace(post.wp_slug) ? post.wp_slug : _routeService.CreateSlug(post.title),
				Content = post.description,
				IsPublished = publish,
				Categories = ConvertCategories(post.categories)
			};

			if (post.dateCreated != DateTime.MinValue)
			{
				newPost.PubDate = post.dateCreated;
			}

			await _blog.AddPost(newPost);

			return newPost.Id.ToString();
		}

		public async Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
		{
			ValidateUser(username, password);

			if(!Guid.TryParse(postid, out Guid postGuid))
			{
				return false;
			}

			var post = await _blog.GetPostById(postGuid);

			if (post != null)
			{
				await _blog.DeletePost(postGuid);
				return true;
			}

			return false;
		}

		public async Task<bool> EditPostAsync(string postid, string username, string password, WilderMinds.MetaWeblog.Post post, bool publish)
		{
			ValidateUser(username, password);

			if(!Guid.TryParse(postid, out Guid postGuid))
			{
				return false;
			}

			var existing = await _blog.GetPostById(postGuid);

			if (existing != null)
			{
				existing.Title = post.title;
				existing.Slug = post.wp_slug;
				existing.Content = post.description;
				existing.IsPublished = publish;
				existing.Categories = ConvertCategories(post.categories);

				if (post.dateCreated != DateTime.MinValue)
				{
					existing.PubDate = post.dateCreated;
				}

				await _blog.UpdatePost(existing);

				return true;
			}

			return false;
		}

		public async Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
		{
			ValidateUser(username, password);

			return (await _blog.GetCategories())
						   .Select(cat =>
							   new CategoryInfo
							   {
								   categoryid = cat.Id.ToString(),
								   title = cat.Name
							   })
						   .ToArray();
		}

		public async Task<WilderMinds.MetaWeblog.Post> GetPostAsync(string postid, string username, string password)
		{
			ValidateUser(username, password);

			if(!Guid.TryParse(postid, out Guid postGuid))
			{
				return null;
			}

			var post = await _blog.GetPostById(postGuid);

			if (post != null)
			{
				return ToMetaWebLogPost(post);
			}

			return null;
		}

		public async Task<WilderMinds.MetaWeblog.Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
		{
			ValidateUser(username, password);

			return (await _blog.GetPosts(numberOfPosts)).Select(ToMetaWebLogPost).ToArray();
		}

		public Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
		{
			ValidateUser(username, password);

			var request = _context.HttpContext.Request;
			string url = request.Scheme + "://" + request.Host;

			return Task.FromResult(new[] { new BlogInfo {
				blogid ="1",
				blogName = _config["blog:name"] ?? nameof(MetaWeblogService),
				url = url
			}});
		}

		public async Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password, MediaObject mediaObject)
		{
			ValidateUser(username, password);
			byte[] bytes = Convert.FromBase64String(mediaObject.bits);
			string path = await _filePersisterService.SaveFile(bytes, mediaObject.name);

			return new MediaObjectInfo { url = path };
		}

		public Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public Task<Page> GetPageAsync(string blogid, string pageid, string username, string password)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public Task<Page[]> GetPagesAsync(string blogid, string username, string password, int numPages)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public Task<Author[]> GetAuthorsAsync(string blogid, string username, string password)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public Task<string> AddPageAsync(string blogid, string username, string password, Page page, bool publish)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public Task<bool> EditPageAsync(string blogid, string pageid, string username, string password, Page page, bool publish)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public Task<bool> DeletePageAsync(string blogid, string username, string password, string pageid)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		private void ValidateUser(string username, string password)
		{
			if (_userServices.ValidateUser(username, password) == false)
			{
				throw new MetaWeblogException("Unauthorized");
			}

			var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
			identity.AddClaim(new Claim(ClaimTypes.Name, username));

			_context.HttpContext.User = new ClaimsPrincipal(identity);
		}

		private WilderMinds.MetaWeblog.Post ToMetaWebLogPost(IPost post)
		{
			return new WilderMinds.MetaWeblog.Post
			{
				postid = post.Id.ToString(),
				title = post.Title,
				wp_slug = post.Slug,
				permalink = _routeService.GetAbsoluteLink(post),
				dateCreated = post.PubDate,
				description = post.Content,
				categories = post.Categories.Select(c => c.Name).ToArray()
			};
		}

		private IList<ICategory> ConvertCategories(string[] categories)
		{
			var categoryList = new List<ICategory>();
			if(categories == null || categories.Length <= 0)
			{
				return categoryList;
			}

			foreach(var category in categories)
			{
				categoryList.Add(new CategoryDto() 
				{
					Id = Guid.NewGuid(),
					Name = category
				});
			}

			return categoryList;
		}

		
	}
}
