using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Services
{
	public class HtmlRenderService : RenderServiceBase
	{
		private readonly IFilePersisterService _filePersisterService;

		public HtmlRenderService(IFilePersisterService filePersisterService)
		{
			_filePersisterService = filePersisterService;
		}

		public override string RenderContent(IPost post)
		{
			var result = post.Content;
			if (!string.IsNullOrEmpty(result))
			{
				// Set up lazy loading of images/iframes
				result = result.Replace(" src=\"", " src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" data-src=\"");

				// Youtube content embedded using this syntax: [youtube:xyzAbc123]
				var video = "<div class=\"video\"><iframe width=\"560\" height=\"315\" title=\"YouTube embed\" src=\"about:blank\" data-src=\"https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&amp;hd=1&amp;rel=0&amp;theme=light\" allowfullscreen></iframe></div>";
				result = Regex.Replace(result, @"\[youtube:(.*?)\]", m => string.Format(video, m.Groups[1].Value));
			}
			return result;
		}

		public override async Task SaveImagesAndReplace(IPost post)
		{
			var imgRegex = new Regex("<img[^>].+ />", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			var base64Regex = new Regex("data:[^/]+/(?<ext>[a-z]+);base64,(?<base64>.+)", RegexOptions.IgnoreCase);

			foreach (Match match in imgRegex.Matches(post.Content))
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml("<root>" + match.Value + "</root>");

				var img = doc.FirstChild.FirstChild;
				var srcNode = img.Attributes["src"];
				var fileNameNode = img.Attributes["data-filename"];

				// The HTML editor creates base64 DataURIs which we'll have to convert to image files on disk
				if (srcNode != null && fileNameNode != null)
				{
					var base64Match = base64Regex.Match(srcNode.Value);
					if (base64Match.Success)
					{
						byte[] bytes = Convert.FromBase64String(base64Match.Groups["base64"].Value);
						srcNode.Value = await _filePersisterService.SaveFile(bytes, fileNameNode.Value).ConfigureAwait(false);

						img.Attributes.Remove(fileNameNode);
						post.Content = post.Content.Replace(match.Value, img.OuterXml);
					}
				}
			}
		}


	}
}