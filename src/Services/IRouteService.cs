using Miniblog.Core.Models;

namespace Miniblog.Core.Services
{
	public interface IRouteService
    {
         string CreateSlug(string title);

		 string GetLink(Post post);

		 string GetAbsoluteLink(Post post);

		 string GetEncodedLink(Post post);

		 string GetAbsoluteEncodedLink(Post post);

		 string GetCommentLink(Post post);

		 string GetCommentLink(Post post, Comment comment);

		 string GetHost();
    }
}