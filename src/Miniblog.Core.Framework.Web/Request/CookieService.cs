using System;
using Microsoft.AspNetCore.Http;

namespace Miniblog.Core.Framework.Web.Request
{
	public class CookieService : ICookieService 
	{
		public IHttpContextAccessor _contextAccessor;
		public CookieService (IHttpContextAccessor contextAccessor) 
		{
			_contextAccessor = contextAccessor;
		}
		public bool CookieHasValue (string name, string value) 
		{
			string cookieValue = _contextAccessor.HttpContext.Request.Cookies[name];
			return string.Equals (cookieValue, value, StringComparison.InvariantCultureIgnoreCase);
		}

		public string GetCookieValue (string name) 
		{
			return _contextAccessor.HttpContext.Request.Cookies[name];
		}
	}
}