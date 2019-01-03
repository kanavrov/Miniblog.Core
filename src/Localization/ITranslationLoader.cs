using System.Collections.Generic;
using System.Globalization;

namespace Miniblog.Core.Localization
{

	public interface ITranslationLoader
	{
		IDictionary<CultureInfo, ITranslationModel> Load();
	}
}