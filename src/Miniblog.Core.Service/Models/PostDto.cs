using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Models
{
	public class PostDto : IPost
	{
		public Guid Id { get; set; }

		[Required]
		public string Title { get; set; }

		public string Slug { get; set; }

		[Required]
		public string Excerpt { get; set; }

		[Required]
		public string Content { get; set; }

		public DateTime PubDate { get; set; }

		public DateTime LastModified { get; set; }

		public bool IsPublished { get; set; } = true;

		public IList<ICategory> Categories { get; set; } = new List<ICategory>();

		public IList<IComment> Comments { get; set; } = new List<IComment>();
	}
}