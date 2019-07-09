using System.Threading.Tasks;
using Markdig;
using Miniblog.Core.Contract.Models;
using Miniblog.Core.Service.Rendering.MarkdownExtensions;

namespace Miniblog.Core.Service.Rendering
{
	public class MarkdownRenderService : RenderServiceBase
	{
		private readonly MarkdownPipeline _pipeline;
		public MarkdownRenderService(IYouTubeEmbedSettings embedSettings, IImageRenderSettings imageSettings)
		{
			_pipeline = new MarkdownPipelineBuilder()
				.Use(new YouTubeLinkExtension(embedSettings))
				.Use(new LazyLoadImageExtension(imageSettings))
				.UsePipeTables().Build();
		}

		public override string RenderContent(IPost post)
		{
			var result = Markdown.ToHtml(post.Content, _pipeline);
			return result;
		}		
	}
}