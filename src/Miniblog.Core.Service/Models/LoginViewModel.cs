using System.ComponentModel.DataAnnotations;

namespace Miniblog.Core.Service.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = ValidationConstants.FieldRequired)]
		[Display(Name = "Login.Username")]
		public string UserName { get; set; }

		[Required(ErrorMessage = ValidationConstants.FieldRequired)]
		[DataType(DataType.Password)]
		[Display(Name = "Login.Password")]
		public string Password { get; set; }

		[Display(Name = "Login.RememberMe")]
		public bool RememberMe { get; set; }
	}
}
