namespace Miniblog.Core.Users
{
	public interface IUserRoleResolver
	{
		bool IsAdmin();
	}
}