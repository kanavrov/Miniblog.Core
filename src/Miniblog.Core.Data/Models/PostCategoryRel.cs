using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Data.Models
{
	public class PostCategoryRel : Category
	{
		public long PostPKeyId { get; set; }

		public long CategoryPKeyId { get; set; }
	}
}