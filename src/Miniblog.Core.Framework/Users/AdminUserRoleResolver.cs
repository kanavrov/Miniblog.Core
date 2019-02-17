namespace Miniblog.Core.Framework.Users
{
	public class AdminUserRoleResolver : IUserRoleResolver
	{
		public bool IsAdmin()
		{
			return true;
		}
	}
}