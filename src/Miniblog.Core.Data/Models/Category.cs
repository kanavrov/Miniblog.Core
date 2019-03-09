using System;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Data.Models
{
	public class Category : EntityBase, ICategory
	{
		public string Name { get; set; }

		public int PostCount { get; set; }
	}
}