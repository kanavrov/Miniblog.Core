using System;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Data.Models
{
	public class Comment : EntityBase, IComment
	{
		public string Author { get; set; }
		public string Email { get; set; }
		public string Content { get; set; }
		public DateTime PubDate { get; set; }
		public bool IsAdmin { get; set; }
		public long PostPKeyId { get; set; }
	}
}