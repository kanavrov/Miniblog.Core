namespace Miniblog.Core.Framework.Web.Request
{
	public interface ICookieService
    {
        bool CookieHasValue(string name, string value);

		string GetCookieValue(string name);
    }
}