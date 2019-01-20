using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Miniblog.Core.Web.Repositories;
using WilderMinds.MetaWeblog;

namespace Miniblog.Core.Web.Services
{
	public class MetaWeblogService : IMetaWeblogProvider
	{
		private readonly IBlogRepository _blog;
		private readonly IConfiguration _config;
		private readonly IUserServices _userServices;
		private readonly IFilePersisterService _filePersisterService;
		private readonly IHttpContextAccessor _context;
		private readonly IRouteService _routeService;

		public MetaWeblogService(IBlogRepository blog, IConfiguration config, IHttpContextAccessor context,
		IUserServices userServices, IFilePersisterService filePersisterService, IRouteService routeService)
		{
			_routeService = routeService;
			_blog = blog;
			_config = config;
			_userServices = userServices;
			_filePersisterService = filePersisterService;
			_context = context;
		}

		public string AddPost(string blogid, string username, string password, WilderMinds.MetaWeblog.Post post, bool publish)
		{
			ValidateUser(username, password);

			var newPost = new Models.Post
			{
				Title = post.title,
				Slug = !string.IsNullOrWhiteSpace(post.wp_slug) ? post.wp_slug : _routeService.CreateSlug(post.title),
				Content = post.description,
				IsPublished = publish,
				Categories = post.categories
			};

			if (post.dateCreated != DateTime.MinValue)
			{
				newPost.PubDate = post.dateCreated;
			}

			_blog.AddPost(newPost).GetAwaiter().GetResult();

			return newPost.ID;
		}

		public bool DeletePost(string key, string postid, string username, string password, bool publish)
		{
			ValidateUser(username, password);

			var post = _blog.GetPostById(postid).GetAwaiter().GetResult();

			if (post != null)
			{
				_blog.DeletePost(postid).GetAwaiter().GetResult();
				return true;
			}

			return false;
		}

		public bool EditPost(string postid, string username, string password, WilderMinds.MetaWeblog.Post post, bool publish)
		{
			ValidateUser(username, password);

			var existing = _blog.GetPostById(postid).GetAwaiter().GetResult();

			if (existing != null)
			{
				existing.Title = post.title;
				existing.Slug = post.wp_slug;
				existing.Content = post.description;
				existing.IsPublished = publish;
				existing.Categories = post.categories;

				if (post.dateCreated != DateTime.MinValue)
				{
					existing.PubDate = post.dateCreated;
				}

				_blog.UpdatePost(existing).GetAwaiter().GetResult();

				return true;
			}

			return false;
		}

		public CategoryInfo[] GetCategories(string blogid, string username, string password)
		{
			ValidateUser(username, password);

			return _blog.GetCategories().GetAwaiter().GetResult()
						   .Select(cat =>
							   new CategoryInfo
							   {
								   categoryid = cat,
								   title = cat
							   })
						   .ToArray();
		}

		public WilderMinds.MetaWeblog.Post GetPost(string postid, string username, string password)
		{
			ValidateUser(username, password);

			var post = _blog.GetPostById(postid).GetAwaiter().GetResult();

			if (post != null)
			{
				return ToMetaWebLogPost(post);
			}

			return null;
		}

		public WilderMinds.MetaWeblog.Post[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
		{
			ValidateUser(username, password);

			return _blog.GetPosts(numberOfPosts).GetAwaiter().GetResult().Select(ToMetaWebLogPost).ToArray();
		}

		public BlogInfo[] GetUsersBlogs(string key, string username, string password)
		{
			ValidateUser(username, password);

			var request = _context.HttpContext.Request;
			string url = request.Scheme + "://" + request.Host;

			return new[] { new BlogInfo {
				blogid ="1",
				blogName = _config["blog:name"] ?? nameof(MetaWeblogService),
				url = url
			}};
		}

		public MediaObjectInfo NewMediaObject(string blogid, string username, string password, MediaObject mediaObject)
		{
			ValidateUser(username, password);
			byte[] bytes = Convert.FromBase64String(mediaObject.bits);
			string path = _filePersisterService.SaveFile(bytes, mediaObject.name).GetAwaiter().GetResult();

			return new MediaObjectInfo { url = path };
		}

		public UserInfo GetUserInfo(string key, string username, string password)
		{
			ValidateUser(username, password);
			throw new NotImplementedException();
		}

		public int AddCategory(string key, string username, string password, NewCategory category)
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

		private WilderMinds.MetaWeblog.Post ToMetaWebLogPost(Models.Post post)
		{
			return new WilderMinds.MetaWeblog.Post
			{
				postid = post.ID,
				title = post.Title,
				wp_slug = post.Slug,
				permalink = _routeService.GetAbsoluteLink(post),
				dateCreated = post.PubDate,
				description = post.Content,
				categories = post.Categories.ToArray()
			};
		}
	}
}
