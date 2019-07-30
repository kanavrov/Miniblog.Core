using System.Collections.Generic;
using System.Globalization;

namespace Miniblog.Core.Framework.Localization
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
			ITranslationModel model = GetAllTranslations(culture);
			if (model != null && model.ContainsKey(key))
			{
				return args != null && args.Length > 0 
					? string.Format(model[key], args) 
					: model[key];
			}

			return key;
		}

		public ITranslationModel GetAllTranslations(string prefix = null)
		{
			return GetAllTranslations(CultureInfo.CurrentCulture, prefix);
		}

		public ITranslationModel GetAllTranslations(CultureInfo culture, string prefix = null)
		{
			if(TranslationStore.LoadedTranslations.ContainsKey(culture))
			{
				var model = TranslationStore.LoadedTranslations[culture];

				if(string.IsNullOrEmpty(prefix))
					return model;

				var formattedPrefix = prefix.EndsWith(".") ? prefix : $"{prefix}.";
				var partialModel = new TranslationModel();
				foreach (var item in model)
				{
					if(item.Key.StartsWith(formattedPrefix))
					{
						partialModel[item.Key] = item.Value;
					}
				}
				return partialModel;
			}				

			return null;			
		}
	}
}