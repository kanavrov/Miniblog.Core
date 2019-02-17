using System.Collections.Concurrent;

namespace Miniblog.Core.Framework.Localization
{
	public class TranslationModel : ConcurrentDictionary<string, string>, ITranslationModel
	{
	}
}