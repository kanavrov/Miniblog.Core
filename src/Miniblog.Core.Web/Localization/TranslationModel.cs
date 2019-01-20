using System.Collections.Concurrent;

namespace Miniblog.Core.Web.Localization
{
	public class TranslationModel : ConcurrentDictionary<string, string>, ITranslationModel
	{
	}
}