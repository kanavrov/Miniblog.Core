using Microsoft.AspNetCore.Http;
using Miniblog.Core.Framework.Users;

namespace Miniblog.Core.Framework.Web.Users
{
	public class IdentityUserRoleResolver : IUserRoleResolver
	{
		private readonly IHttpContextAccessor _contextAccessor;

		public IdentityUserRoleResolver(IHttpContextAccessor contextAccessor)
		{
			_contextAccessor = contextAccessor;
		}

		public bool IsAdmin()
		{
			return _contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
		}
	}
}