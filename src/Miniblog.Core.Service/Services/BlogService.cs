using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Miniblog.Core.Contract.Models;
using Miniblog.Core.Data.Repositories;
using Miniblog.Core.Framework.Users;
using Miniblog.Core.Service.Models;
using Miniblog.Core.Service.Settings;
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

namespace Miniblog.Core.Service.Services
{
	public class BlogService : IBlogService
	{
		private readonly IBlogRepository _blogRepository;
		private readonly IOptions<BlogSettings> _settings;
		private readonly IFilePersisterService _filePersisterService;
		private readonly IUserRoleResolver _userRoleResolver;
		private readonly IRenderService _renderService;
		public readonly IRouteService _routeService;
		private readonly IMapper _mapper;

		public BlogService(IHostingEnvironment env, IBlogRepository blogRepository,
		IUserRoleResolver userRoleResolver, IOptions<BlogSettings> settings,
		IFilePersisterService filePersisterService, IRenderService renderService,
		IRouteService routeService, IMapper mapper)
		{
			_renderService = renderService;
			_routeService = routeService;
			_mapper = mapper;
			_blogRepository = blogRepository;
			_settings = settings;
			_filePersisterService = filePersisterService;
			_userRoleResolver = userRoleResolver;
		}

		public virtual async Task<IEnumerable<PostDto>> GetPosts(int page = 0)
		{
			var pageModel = GetPageModel(page);
			return _mapper.Map<IEnumerable<PostDto>>(await _blogRepository.GetPosts(pageModel.ItemCount, pageModel.SkipCount));
		}

		public virtual async Task<IEnumerable<PostDto>> GetPostsByCategory(Guid categoryId, int page = 0)
		{
			var pageModel = GetPageModel(page);
			return _mapper.Map<IEnumerable<PostDto>>(await _blogRepository.GetPostsByCategory(categoryId, pageModel.ItemCount, pageModel.SkipCount));
		}

		public virtual async Task<PostDto> GetPostBySlug(string slug)
		{
			return _mapper.Map<PostDto>(await _blogRepository.GetPostBySlug(slug));
		}

		public virtual async Task<PostDto> GetPostById(Guid id)
		{
			return _mapper.Map<PostDto>(await _blogRepository.GetPostById(id));
		}

		public virtual async Task<IEnumerable<CategoryDto>> GetCategories()
		{
			return _mapper.Map<IEnumerable<CategoryDto>>((await _blogRepository.GetCategories()).OrderBy(c => c.Name));
		}

		public virtual async Task SavePost(PostDto post, Guid[] categories)
		{
			var isNew = false;
			var existing = _mapper.Map<PostDto>(await GetPostById(post.Id));

			if (existing == null)
			{
				existing = post;
				isNew = true;
			}

			existing.Categories = categories != null ? categories.Select(c => (new CategoryDto { Id = c } as ICategory)).ToList() : new List<ICategory>();
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

		public virtual async Task DeletePost(Guid id)
		{
			await _blogRepository.DeletePost(id);
		}

		public virtual async Task AddComment(CommentDto comment, Guid postId)
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

		public virtual async Task DeleteComment(Guid commentId, Guid postId)
		{
			await _blogRepository.DeleteComment(commentId, postId);
		}

		public virtual async Task<CategoryDto> GetCategoryById(Guid id)
		{
			return _mapper.Map<CategoryDto>(await _blogRepository.GetCategoryById(id));
		}

		public virtual async Task AddCategory(CategoryDto category)
		{
			await _blogRepository.AddCategory(category);
		}

		public virtual async Task UpdateCategory(CategoryDto category)
		{
			await _blogRepository.UpdateCategory(category);
		}

		public virtual async Task DeleteCategory(Guid id)
		{
			await _blogRepository.DeleteCategory(id);
		}

		private PageModel GetPageModel(int page)
		{
			page = page >= 0 ? page : 0;
			
			return new PageModel()
			{
				ItemCount = _settings.Value.PostsPerPage,
				SkipCount = _settings.Value.PostsPerPage * page
			};
		}

		
	}
}