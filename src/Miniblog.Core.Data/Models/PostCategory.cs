using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Data.Models
{
	public class PostCategory : Category
	{
		public int PostPKeyId { get; set; }
	}
}