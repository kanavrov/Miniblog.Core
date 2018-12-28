using Microsoft.AspNetCore.Http;

namespace Miniblog.Core.Users
{
    public class CurrentUserRoleResolver : IUserRoleResolver
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CurrentUserRoleResolver(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public bool IsAdmin()
        {
            return _contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
        }
    }
}