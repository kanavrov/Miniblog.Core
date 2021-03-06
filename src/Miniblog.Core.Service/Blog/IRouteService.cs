using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Blog
{
	public interface IRouteService
    {
         string CreateSlug(string title);

		 string GetLink(IPost post);

		 string GetPostLink(string slug);

		 string GetAbsoluteLink(IPost post);

		 string GetEncodedLink(IPost post);

		 string GetAbsoluteEncodedLink(IPost post);

		 string GetCommentLink(IPost post);

		 string GetCommentLink(IPost post, IComment comment);

		 string GetHost();
    }
}