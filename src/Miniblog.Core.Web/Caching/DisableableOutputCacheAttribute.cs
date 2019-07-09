using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Miniblog.Core.Service.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Miniblog.Core.Web.Caching
{
	public class DisableableOutputCacheAttribute : OutputCacheAttribute
	{
		public DisableableOutputCacheAttribute(params string[] fileDependencies)
			: base(fileDependencies)
		{
		}

		public override void OnActionExecuting(ActionExecutingContext context) 
		{
			var developmentSettings = context.HttpContext.RequestServices.GetService<IOptionsSnapshot<DevelopmentSettings>>();

			if(!developmentSettings.Value.OutputCachingDisabled)
			{
				base.OnActionExecuting(context);
			}
		}
	}
}