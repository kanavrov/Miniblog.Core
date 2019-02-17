using System;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Data.Models
{
	public class Category : ICategory
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}