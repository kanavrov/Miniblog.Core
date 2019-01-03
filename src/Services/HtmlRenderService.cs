using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Miniblog.Core.Models;

namespace Miniblog.Core.Services
{
	public class HtmlRenderService : IRenderService
	{
		private readonly IFilePersisterService _filePersisterService;

		public HtmlRenderService(IFilePersisterService filePersisterService)
		{
			_filePersisterService = filePersisterService;
		}

		public async Task SaveImagesAndReplace(Post post)
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