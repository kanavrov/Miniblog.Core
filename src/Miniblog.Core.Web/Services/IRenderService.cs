using System.Threading.Tasks;
using Miniblog.Core.Web.Models;

namespace Miniblog.Core.Web.Services
{
	public interface IRenderService
	{
		string RenderContent(Post post);

		string RenderContent(Comment comment);
		
		Task SaveImagesAndReplace(Post post);

		string GetGravatar(Comment comment);
	}
}