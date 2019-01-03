using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Miniblog.Core.Models;

namespace Miniblog.Core.Services
{
	public class BlogRouteService : IRouteService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		public BlogRouteService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}
		public string CreateSlug(string title)
		{
			title = title.ToLowerInvariant().Replace(" ", "-");
			title = RemoveDiacritics(title);
			title = RemoveReservedUrlCharacters(title);

			return title.ToLowerInvariant();
		}

		public string GetHost()
		{
			var request = _httpContextAccessor.HttpContext.Request;
			return request.Scheme + "://" + request.Host;
		}

		public string GetEncodedLink(Post post)
		{
			return $"/blog/{System.Net.WebUtility.UrlEncode(post.Slug)}/";
		}

		public string GetAbsoluteEncodedLink(Post post)
		{
			return GetHost() + GetEncodedLink(post);
		}

		public string GetCommentLink(Post post)
		{
			return GetEncodedLink(post) + "#comments";
		}

		public string GetCommentLink(Post post, Comment comment)
		{
			return GetEncodedLink(post) + $"#{comment.ID}";
		}
		public string GetLink(Post post)
		{
			return $"/blog/{post.Slug}/";
		}

		public string GetAbsoluteLink(Post post)
		{
			return GetHost() + GetLink(post);
		}

		private static string RemoveReservedUrlCharacters(string text)
		{
			var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };

			foreach (var chr in reservedCharacters)
			{
				text = text.Replace(chr, "");
			}

			return text;
		}

		private static string RemoveDiacritics(string text)
		{
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}
	}
}