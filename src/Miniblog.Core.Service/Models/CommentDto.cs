using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Models
{
	public class CommentDto : IComment
	{
		public Guid Id { get; set; }

		[Required]
		public string Author { get; set; }

		[Required, EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Content { get; set; }

		public DateTime PubDate { get; set; }

		public bool IsAdmin { get; set; }		
	}
}
