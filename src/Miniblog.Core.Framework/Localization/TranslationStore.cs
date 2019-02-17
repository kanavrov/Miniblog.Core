using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace Miniblog.Core.Framework.Localization
{
	public class TranslationStore : ITranslationStore
    {
	    public ITranslationLoader TranslationLoader { get; }
	    public IDictionary<CultureInfo, ITranslationModel> LoadedTranslations { get; private set; }

	    public TranslationStore(ITranslationLoader translationLoader)
	    {
		    TranslationLoader = translationLoader;
			LoadedTranslations = new ConcurrentDictionary<CultureInfo, ITranslationModel>();
	    }

		public void Load()
		{
			LoadedTranslations = new ConcurrentDictionary<CultureInfo, ITranslationModel>(TranslationLoader.Load());
		}
    }
}