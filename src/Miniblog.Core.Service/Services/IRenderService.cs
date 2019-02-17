using System.Threading.Tasks;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Services
{
	public interface IRenderService
	{
		string RenderContent(IPost post);

		string RenderContent(IComment comment);
		
		Task SaveImagesAndReplace(IPost post);

		string GetGravatar(IComment comment);
	}
}