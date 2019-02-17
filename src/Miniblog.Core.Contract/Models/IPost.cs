using System;
using System.Collections.Generic;

namespace Miniblog.Core.Contract.Models
{
	public interface IPost
    {
        Guid Id { get; set; }

		string Title { get; set; }

		string Slug { get; set; }

		string Excerpt { get; set; }

		string Content { get; set; }

		DateTime PubDate { get; set; }

		DateTime LastModified { get; set; }

		bool IsPublished { get; set; }

		IList<ICategory> Categories { get; set; }

		IList<IComment> Comments { get; set; }
    }
}