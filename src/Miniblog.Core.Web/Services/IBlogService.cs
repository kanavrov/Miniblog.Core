using System.Collections.Generic;
using System.Threading.Tasks;
using Miniblog.Core.Web.Models;

namespace Miniblog.Core.Web.Services
{
	public interface IBlogService
	{
		Task<IEnumerable<Post>> GetPosts(int count, int skip = 0);

		Task<IEnumerable<Post>> GetPostsByCategory(string category);

		Task<Post> GetPostBySlug(string slug);

		Task<Post> GetPostById(string id);

		Task<IEnumerable<string>> GetCategories();

		Task SavePost(Post post, string categories);

		Task DeletePost(string id);

		Task AddComment(Comment comment, string postId);

		Task DeleteComment(string commentId, string postId);
	}
}
