namespace Miniblog.Core.Web.Services
{
	public interface IUserServices
	{
		bool ValidateUser(string username, string password);
	}
}
