using System;
using System.Collections.Generic;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Data.Models
{
	public class Post : EntityBase, IPost
	{
		public string Title { get; set; }
		public string Slug { get; set; }
		public string Excerpt { get; set; }
		public string Content { get; set; }
		public DateTime PubDate { get; set; }
		public DateTime LastModified { get; set; }
		public bool IsPublished { get; set; }
		public IList<ICategory> Categories { get; set; }
		public IList<IComment> Comments { get; set; }
	}
}