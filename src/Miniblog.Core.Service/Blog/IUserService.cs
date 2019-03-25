namespace Miniblog.Core.Service.Blog
{
	public interface IUserService
	{
		bool ValidateUser(string username, string password);
	}
}
