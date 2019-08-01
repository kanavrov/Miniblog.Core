using System;
using System.ComponentModel.DataAnnotations;
using Miniblog.Core.Contract.Models;

namespace Miniblog.Core.Service.Models
{
	public class CategoryDto : ICategory
	{
		public Guid Id { get; set; }
		
		[Required(ErrorMessage = ValidationConstants.FieldRequired)]
		[Display(Name = "EditCategory.CategoryName")]
		public string Name { get; set; }

		public DateTime LastModified { get; set; }
		
		public int PostCount { get; set; }
	}
}