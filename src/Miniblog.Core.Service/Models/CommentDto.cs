using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Models
{
	public class CommentDto : IComment
	{
		public Guid Id { get; set; }

		[Required(ErrorMessage = ValidationConstants.FieldRequired)]
		[Display(Name = "Comments.Name")]
		public string Author { get; set; }

		[Required(ErrorMessage = ValidationConstants.FieldRequired)]
		[EmailAddress(ErrorMessage = ValidationConstants.Email)]
		[Display(Name = "Comments.Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = ValidationConstants.FieldRequired)]
		[Display(Name = "Comments.Comment")]
		public string Content { get; set; }

		public DateTime PubDate { get; set; }

		public bool IsAdmin { get; set; }		
	}
}
