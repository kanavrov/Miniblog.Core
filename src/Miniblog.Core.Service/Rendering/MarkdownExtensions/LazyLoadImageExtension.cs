using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Miniblog.Core.Service.Rendering.MarkdownExtensions
{
	public class LazyLoadImageExtension : IMarkdownExtension
	{
		public LazyLoadImageExtension(IImageRenderSettings settings) 
		{
			Settings = settings;
		}

		private IImageRenderSettings Settings { get; }

		public void Setup(MarkdownPipelineBuilder pipeline)
		{
		}

		public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
		{
			var htmlRenderer = renderer as HtmlRenderer;
			if (htmlRenderer != null)
			{
				var inlineRenderer = htmlRenderer.ObjectRenderers.FindExact<LinkInlineRenderer>();
				if (inlineRenderer != null)
				{
					inlineRenderer.TryWriters.Remove(TryLinkInlineRenderer);
					inlineRenderer.TryWriters.Add(TryLinkInlineRenderer);
				}
			}
		}

		private bool TryLinkInlineRenderer(HtmlRenderer renderer, LinkInline linkInline)
		{
			if (!linkInline.IsImage || linkInline.Url == null)
				return false;
			
			if (TryRenderImage(renderer, linkInline))
				return true;
			
			return false;
		}

		private bool TryRenderImage(HtmlRenderer renderer, LinkInline link)
		{
			if (renderer.EnableHtmlForInline)
			{
				var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

				if (string.IsNullOrEmpty(url))
					return false;

				renderer.Write("<img data-src=\"");
				renderer.WriteEscapeUrl(link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url);
				renderer.Write($"\" src=\"{Settings.Placeholder}\"");
				renderer.WriteAttributes(link);
				renderer.Write(" alt=\"");
			}

			var wasEnableHtmlForInline = renderer.EnableHtmlForInline;
			renderer.EnableHtmlForInline = false;
			renderer.WriteChildren(link);
			renderer.EnableHtmlForInline = wasEnableHtmlForInline;

			if (renderer.EnableHtmlForInline)
				renderer.Write("\" />");
			
			return true;
		}

	}
}