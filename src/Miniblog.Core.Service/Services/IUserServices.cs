namespace Miniblog.Core.Service.Services
{
	public interface IUserServices
	{
		bool ValidateUser(string username, string password);
	}
}
