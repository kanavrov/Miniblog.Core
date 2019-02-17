using System;

namespace Miniblog.Core.Contract.Models
{
	public interface IComment
	{
		Guid Id { get; set; }

		string Author { get; set; }

		string Email { get; set; }

		string Content { get; set; }

		DateTime PubDate { get; set; }

		bool IsAdmin { get; set; }
	}
}