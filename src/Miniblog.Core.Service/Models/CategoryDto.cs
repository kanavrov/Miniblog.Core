using System;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Models
{
	public class CategoryDto : ICategory
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}