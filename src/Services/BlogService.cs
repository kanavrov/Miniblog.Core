using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Miniblog.Core.Models;
using Miniblog.Core.Repositories;
using Miniblog.Core.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Miniblog.Core.Services
{
	public class BlogService : IBlogService
	{
		private readonly IBlogRepository _blogRepository;
		private readonly IOptions<BlogSettings> _settings;
		private readonly IFilePersisterService _filePersisterService;
		private readonly IUserRoleResolver _userRoleResolver;
		private readonly IRenderService _renderService;
		public readonly IRouteService _routeService;

		public BlogService(IHostingEnvironment env, IBlogRepository blogRepository,
		IUserRoleResolver userRoleResolver, IOptions<BlogSettings> settings,
		IFilePersisterService filePersisterService, IRenderService renderService,
		IRouteService routeService)
		{
			_renderService = renderService;
			_routeService = routeService;
			_blogRepository = blogRepository;
			_settings = settings;
			_filePersisterService = filePersisterService;
			_userRoleResolver = userRoleResolver;
		}

		public virtual async Task<IEnumerable<Post>> GetPosts(int count, int skip = 0)
		{
			return await _blogRepository.GetPosts(count, skip);
		}

		public virtual async Task<IEnumerable<Post>> GetPostsByCategory(string category)
		{
			return await _blogRepository.GetPostsByCategory(category);
		}

		public virtual async Task<Post> GetPostBySlug(string slug)
		{
			return await _blogRepository.GetPostBySlug(slug);
		}

		public virtual async Task<Post> GetPostById(string id)
		{
			return await _blogRepository.GetPostById(id);
		}

		public virtual async Task<IEnumerable<string>> GetCategories()
		{
			return await _blogRepository.GetCategories();
		}

		public async Task SavePost(Post post, string categories)
		{
			var isNew = false;
			var existing = await GetPostById(post.ID);

			if (existing == null)
			{
				existing = post;
				isNew = true;
			}

			existing.Categories = categories.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim().ToLowerInvariant()).ToList();
			existing.Title = post.Title.Trim();
			existing.Slug = !string.IsNullOrWhiteSpace(post.Slug) ? post.Slug.Trim() : _routeService.CreateSlug(post.Title);
			existing.IsPublished = post.IsPublished;
			existing.Content = post.Content.Trim();
			existing.Excerpt = post.Excerpt.Trim();

			await _renderService.SaveImagesAndReplace(existing);

			if (isNew)
			{
				await _blogRepository.AddPost(post);
				return;
			}

			await _blogRepository.UpdatePost(post);
		}

		public async Task DeletePost(string id)
		{
			await _blogRepository.DeletePost(id);
		}

		public async Task AddComment(Comment comment, string postId)
		{
			var post = await GetPostById(postId);

			if (!post.AreCommentsOpen(_settings.Value.CommentsCloseAfterDays))
			{
				return;
			}

			comment.IsAdmin = _userRoleResolver.IsAdmin();
			comment.Content = comment.Content.Trim();
			comment.Author = comment.Author.Trim();
			comment.Email = comment.Email.Trim();

			await _blogRepository.AddComment(comment, postId);
		}

		public async Task DeleteComment(string commentId, string postId)
		{
			await _blogRepository.DeleteComment(commentId, postId);
		}
	}
}