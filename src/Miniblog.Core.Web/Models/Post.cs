using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Miniblog.Core.Web.Models
{
	public class Post
	{
		[Required]
		public string ID { get; set; } = DateTime.UtcNow.Ticks.ToString();

		[Required]
		public string Title { get; set; }

		public string Slug { get; set; }

		[Required]
		public string Excerpt { get; set; }

		[Required]
		public string Content { get; set; }

		public DateTime PubDate { get; set; } = DateTime.UtcNow;

		public DateTime LastModified { get; set; } = DateTime.UtcNow;

		public bool IsPublished { get; set; } = true;

		public IList<string> Categories { get; set; } = new List<string>();

		public IList<Comment> Comments { get; } = new List<Comment>();
		public bool AreCommentsOpen(int commentsCloseAfterDays)
		{
			return PubDate.AddDays(commentsCloseAfterDays) >= DateTime.UtcNow;
		}
	}
}