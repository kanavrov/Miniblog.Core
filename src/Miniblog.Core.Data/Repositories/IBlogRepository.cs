using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Miniblog.Core.Contract.Models;
using Miniblog.Core.Data.Models;

namespace Miniblog.Core.Data.Repositories
{
	public interface IBlogRepository
	{
		Task<IEnumerable<IPost>> GetPostsByCategory(Guid categoryId, int count, int skip = 0);

		Task<IEnumerable<IPost>> GetPosts(int count, int skip = 0);

		Task<IPost> GetPostBySlug(string slug);

		Task<IPost> GetPostById(Guid id);

		Task<IEnumerable<ICategory>> GetCategories();

		Task<ICategory> GetCategoryById(Guid id);

		Task AddPost(IPost post);

		Task UpdatePost(IPost post);

		Task DeletePost(Guid id);

		Task AddComment(IComment comment, Guid postId);

		Task DeleteComment(Guid commentId, Guid postId);
	}
}