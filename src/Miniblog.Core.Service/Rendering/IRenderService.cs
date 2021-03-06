using System.Threading.Tasks;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Rendering
{
	public interface IRenderService
	{
		string RenderContent(IPost post);

		string RenderContent(IComment comment);
		
		string GetGravatar(IComment comment);
	}
}