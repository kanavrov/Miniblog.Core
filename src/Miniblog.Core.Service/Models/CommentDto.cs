using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Models
{
	public class CommentDto : IComment
	{
		[Required]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public string Author { get; set; }

		[Required, EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Content { get; set; }

		[Required]
		public DateTime PubDate { get; set; } = DateTime.UtcNow;

		public bool IsAdmin { get; set; }		
	}
}
