namespace Miniblog.Core.Web.Users
{
	public interface IUserRoleResolver
	{
		bool IsAdmin();
	}
}