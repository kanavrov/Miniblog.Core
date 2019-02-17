using System;

namespace Miniblog.Core.Contract.Models
{
	public interface ICategory
	{
		Guid Id { get; set; }

		string Name { get; set; }
	}
}