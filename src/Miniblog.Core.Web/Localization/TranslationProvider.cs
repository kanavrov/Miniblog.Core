using System.Globalization;

namespace Miniblog.Core.Web.Localization
{
	public class TranslationProvider : ITranslationProvider
	{
		public ITranslationStore TranslationStore { get; }

		public TranslationProvider(ITranslationStore translationStore)
		{
			TranslationStore = translationStore;
			TranslationStore.Load();
		}

		public string Translate(string key, params object[] args)
		{
			return Translate(CultureInfo.CurrentCulture, key, args);
		}

		public string Translate(CultureInfo culture, string key, params object[] args)
		{
			if (TranslationStore.LoadedTranslations.ContainsKey(culture))
			{
				ITranslationModel model = TranslationStore.LoadedTranslations[culture];
				if (model.ContainsKey(key))
				{
					return string.Format(model[key], args);
				}
			}
			return key;
		}
	}
}