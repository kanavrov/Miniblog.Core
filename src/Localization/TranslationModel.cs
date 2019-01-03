using System.Collections.Concurrent;

namespace Miniblog.Core.Localization
{
	public class TranslationModel : ConcurrentDictionary<string, string>, ITranslationModel
	{
	}
}