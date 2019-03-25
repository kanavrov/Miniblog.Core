using System.Threading.Tasks;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Services
{
	public class MarkdownRenderService : RenderServiceBase
	{
		private readonly IFilePersisterService _filePersisterService;

		public MarkdownRenderService(IFilePersisterService filePersisterService)
		{
			_filePersisterService = filePersisterService;
		}

		public override string RenderContent(IPost post)
		{
			throw new System.NotImplementedException();
		}

		public override async Task SaveImagesAndReplace(IPost post)
		{
			throw new System.NotImplementedException();
		}
	}
}