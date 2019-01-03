using System.Collections.Generic;
using System.Globalization;

namespace Miniblog.Core.Localization
{
	public interface ITranslationStore
	{
		IDictionary<CultureInfo, ITranslationModel> LoadedTranslations { get; }

		void Load();
	}
}