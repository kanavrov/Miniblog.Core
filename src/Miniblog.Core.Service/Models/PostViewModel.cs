using System.Collections.Generic;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Models
{
	public class PostViewModel 
	{
		public IList<CategoryDto> AllCategories { get; set; }

		public PostDto Post { get; set; }
	}
}