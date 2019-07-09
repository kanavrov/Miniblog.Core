using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Markdig.Extensions.MediaLinks;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Miniblog.Core.Service.Rendering.MarkdownExtensions
{
	public class YouTubeLinkExtension : IMarkdownExtension
	{
		private const string HostPrefix = "www.youtube.com";

		public YouTubeLinkExtension(IYouTubeEmbedSettings settings)
		{
			Settings = settings;
		}

		private IYouTubeEmbedSettings Settings { get; }

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

			// Only process absolute Uri
			if (!Uri.TryCreate(linkInline.Url, UriKind.RelativeOrAbsolute, out Uri uri) || !uri.IsAbsoluteUri)
				return false;

			if (TryRenderIframe(uri, renderer, linkInline))
				return true;

			return false;
		}

		private bool TryRenderIframe(Uri uri, HtmlRenderer renderer, LinkInline linkInline)
		{
			if (!uri.Host.StartsWith(HostPrefix, StringComparison.OrdinalIgnoreCase))
				return false;

			var htmlAttributes = new HtmlAttributes();
			var srcUrl = RenderSourceUrl(uri);

			if (string.IsNullOrEmpty(srcUrl))
				return false;

			renderer.Write($"<div class=\"video\"><iframe src=\"about:blank\" data-src=\"{srcUrl}\"");

			if (Settings.Width > 0)
				htmlAttributes.AddPropertyIfNotExist("width", Settings.Width);

			if (Settings.Height > 0)
				htmlAttributes.AddPropertyIfNotExist("height", Settings.Height);

			if (Settings.AllowFullscreen)
				htmlAttributes.AddPropertyIfNotExist("allowfullscreen", null);

			if (string.IsNullOrEmpty(Settings.Title))
				htmlAttributes.AddPropertyIfNotExist("title", Settings.Title);

			renderer.WriteAttributes(htmlAttributes);
			renderer.Write("></iframe></div>");

			return true;
		}

		private static readonly string[] SplitAnd = { "&" };
		private static string[] SplitQuery(Uri uri)
		{
			var query = uri.Query.Substring(uri.Query.IndexOf('?') + 1);
			return query.Split(SplitAnd, StringSplitOptions.RemoveEmptyEntries);
		}

		private string RenderSourceUrl(Uri uri)
		{
			var query = SplitQuery(uri);
			return query.Length > 0 && query[0].StartsWith("v=")
				? string.Format(Settings.EmbedUrlTemplate, query[0].Substring(2))
				: null;
		}
	}
}