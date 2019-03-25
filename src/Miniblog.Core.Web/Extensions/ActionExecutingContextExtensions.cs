using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Miniblog.Core.Web.Extensions
{
	public static class ActionExecutingContextExtensions
	{
		public static T GetActionAttribute<T>(this ActionExecutingContext context) where T : Attribute
		{
			var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
			if (controllerActionDescriptor != null)
			{
				return controllerActionDescriptor.MethodInfo.GetCustomAttribute<T>(true);
			}
			return null;
		}
	}
}