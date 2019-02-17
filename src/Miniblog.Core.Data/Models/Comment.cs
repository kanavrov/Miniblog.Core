using System;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Data.Models
{
	public class Comment : IComment
	{
		public Guid Id { get; set; }
		public string Author { get; set; }
		public string Email { get; set; }
		public string Content { get; set; }
		public DateTime PubDate { get; set; }
		public bool IsAdmin { get; set; }
	}
}