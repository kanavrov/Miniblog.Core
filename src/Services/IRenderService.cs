using System.Threading.Tasks;
using Miniblog.Core.Models;

namespace Miniblog.Core.Services
{
	public interface IRenderService
	{
		string RenderContent(Post post);

		string RenderContent(Comment comment);
		
		Task SaveImagesAndReplace(Post post);

		string GetGravatar(Comment comment);
	}
}