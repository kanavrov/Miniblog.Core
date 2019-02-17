using System.Collections.Generic;
using System.Globalization;

namespace Miniblog.Core.Framework.Localization
{
	public interface ITranslationStore
	{
		IDictionary<CultureInfo, ITranslationModel> LoadedTranslations { get; }

		void Load();
	}
}