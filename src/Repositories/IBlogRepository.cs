using System.Collections.Generic;
using System.Threading.Tasks;
using Miniblog.Core.Models;

namespace Miniblog.Core.Repositories
{
    public interface IBlogRepository
    {
        Task<IEnumerable<Post>> GetPostsByCategory(string category);
        
        Task<IEnumerable<Post>> GetPosts(int count, int skip = 0);
        
        Task<Post> GetPostBySlug(string slug);
        
        Task<Post> GetPostById(string id);

        Task<IEnumerable<string>> GetCategories();
        
        Task AddPost(Post post);
        
        Task UpdatePost(Post post);
        
        Task DeletePost(string id);
        
        Task AddComment(Comment comment, string postId);
        
        Task DeleteComment(string commentId, string postId);
    }
}