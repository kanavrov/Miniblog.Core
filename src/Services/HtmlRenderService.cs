using System;
using System.Text;
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

		public string GetGravatar(Comment comment)
		{
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(comment.Email.Trim().ToLowerInvariant());
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				var sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					sb.Append(hashBytes[i].ToString("X2"));
				}

				return $"https://www.gravatar.com/avatar/{sb.ToString().ToLowerInvariant()}?s=60&d=blank";
			}
		}

		public string RenderContent(Post post)
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

		public string RenderContent(Comment comment)
		{
			return comment.Content;
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