using System;
using Microsoft.Extensions.Localization;

namespace Miniblog.Core.Framework.Localization
{
	public class CustomStringLocalizerFactory : IStringLocalizerFactory {
		private readonly ITranslationProvider _translationProvider;
		public CustomStringLocalizerFactory (ITranslationProvider translationProvider) 
		{
			_translationProvider = translationProvider;
		}

		public IStringLocalizer Create (Type resourceSource) 
		{
			return Create();
		}

		public IStringLocalizer Create (string baseName, string location) 
		{
			return Create();
		}

		private IStringLocalizer Create () 
		{
			return new CustomStringLocalizer(_translationProvider);
		}
	}
}