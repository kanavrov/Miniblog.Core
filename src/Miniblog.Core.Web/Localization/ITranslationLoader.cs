using System.Collections.Generic;
using System.Globalization;

namespace Miniblog.Core.Web.Localization
{

	public interface ITranslationLoader
	{
		IDictionary<CultureInfo, ITranslationModel> Load();
	}
}