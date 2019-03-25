using System.Threading.Tasks;
using Markdig;
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
			var pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
			var result = Markdown.ToHtml(post.Content, pipeline);
			if (!string.IsNullOrEmpty(result))
			{
				// Set up lazy loading of images/iframes
				result = result.Replace(" src=\"", " src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" data-src=\"");				
			}
			return result;
		}		
	}
}