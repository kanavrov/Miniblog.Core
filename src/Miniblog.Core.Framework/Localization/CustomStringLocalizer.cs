using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace Miniblog.Core.Framework.Localization
{
	/// <summary>
	/// Native wrapper around ITranslationProvider.
	/// </summary>
	public class CustomStringLocalizer : IStringLocalizer {

		private readonly ITranslationProvider _translationProvider;
		public CustomStringLocalizer (ITranslationProvider translationProvider) 
		{
			_translationProvider = translationProvider;
		}

		public LocalizedString this [string name] =>
			CreateString(name, _translationProvider.Translate(name));

		public LocalizedString this [string name, params object[] arguments] =>
			CreateString(name, _translationProvider.Translate(name), arguments);

		public IEnumerable<LocalizedString> GetAllStrings (bool includeParentCultures) 
		{
			return _translationProvider.GetAllTranslations()?.Select(x => CreateString(x.Key, x.Value)) 
				?? new List<LocalizedString>();
		}

		public IStringLocalizer WithCulture (CultureInfo culture) 
		{
			return new CustomStringLocalizer(_translationProvider);
		}

		private LocalizedString CreateString(string key, string value, params object[] arguments)
		{
			return string.IsNullOrEmpty(value) 
				? new LocalizedString(key, key, true) 
				: new LocalizedString(key, string.Format(value, arguments));
		}
	}
}