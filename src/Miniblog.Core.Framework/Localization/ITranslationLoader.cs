using System.Collections.Generic;
using System.Globalization;

namespace Miniblog.Core.Framework.Localization
{

	public interface ITranslationLoader
	{
		IDictionary<CultureInfo, ITranslationModel> Load();
	}
}