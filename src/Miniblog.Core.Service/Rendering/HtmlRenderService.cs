using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Rendering
{
	public class HtmlRenderService : RenderServiceBase
	{
		private readonly IYouTubeEmbedSettings _embedSettings;
		private readonly IImageRenderSettings _imageSettings;

		public HtmlRenderService(IYouTubeEmbedSettings embedSettings, IImageRenderSettings imageSettings)
		{
			_embedSettings = embedSettings;
			_imageSettings = imageSettings;
		}
		public override string RenderContent(IPost post)
		{
			var result = post.Content;
			if (!string.IsNullOrEmpty(result))
			{
				// Set up lazy loading of images/iframes
				result = result.Replace(" src=\"", $" src=\"{_imageSettings.Placeholder}\" data-src=\"");

				// Youtube content embedded using this syntax: [youtube:xyzAbc123]
				var attributes = BuildEmbedAttributes();
				var embedUrlTemplate = HttpUtility.HtmlEncode(_embedSettings.EmbedUrlTemplate);				
				var video = "<div class=\"video\"><iframe {0} src=\"about:blank\" data-src=\"{1}\"></iframe></div>";

				result = Regex.Replace(result, @"\[youtube:(.*?)\]", m => 
				{
					var url = string.Format(embedUrlTemplate, m.Groups[1].Value);
					return string.Format(video, attributes, url);
				});
			}
			return result;
		}

		private string BuildEmbedAttributes()
		{
			return $"width=\"{_embedSettings.Width}\" height=\"{_embedSettings.Height}\" title=\"{_embedSettings.Title}\" {(_embedSettings.AllowFullscreen ? "allowfullscreen" : string.Empty)}";
		}
	}
}