using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

public class NoClientCacheAttribute : ActionFilterAttribute {
	
	public override void OnResultExecuting (ResultExecutingContext context) {
		context.HttpContext.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store";
		context.HttpContext.Response.Headers[HeaderNames.Pragma] = "no-cache";
		context.HttpContext.Response.Headers[HeaderNames.Expires] = "0"; 
	}
}