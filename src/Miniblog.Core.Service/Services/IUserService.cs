namespace Miniblog.Core.Service.Services
{
	public interface IUserService
	{
		bool ValidateUser(string username, string password);
	}
}
