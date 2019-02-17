﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Miniblog.Core.Service.Models;

namespace Miniblog.Core.Service.Services
{
	public interface IBlogService
	{
		Task<IEnumerable<PostDto>> GetPosts(int page = 0);

		Task<IEnumerable<PostDto>> GetPostsByCategory(Guid categoryId, int page = 0);

		Task<PostDto> GetPostBySlug(string slug);

		Task<PostDto> GetPostById(Guid id);

		Task<IEnumerable<CategoryDto>> GetCategories();

		Task<CategoryDto> GetCategoryById(Guid id);

		Task SavePost(PostDto post, string categories);

		Task DeletePost(Guid id);

		Task AddComment(CommentDto comment, Guid postId);

		Task DeleteComment(Guid commentId, Guid postId);
	}
}
