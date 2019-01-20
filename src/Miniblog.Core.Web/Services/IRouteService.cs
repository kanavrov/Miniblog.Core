using Miniblog.Core.Web.Models;

namespace Miniblog.Core.Web.Services
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