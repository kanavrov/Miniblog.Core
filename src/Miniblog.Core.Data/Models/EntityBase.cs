using System;

namespace Miniblog.Core.Data.Models
{
	public abstract class EntityBase
	{
		public long PKeyId { get; set; }

		public Guid Id { get; set; }
	}
}