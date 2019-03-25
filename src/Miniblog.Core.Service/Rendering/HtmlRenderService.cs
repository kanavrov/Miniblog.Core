using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Rendering
{
	public class HtmlRenderService : RenderServiceBase
	{
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
	}
}