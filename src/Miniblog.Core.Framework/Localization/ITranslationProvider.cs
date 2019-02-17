using System.Globalization;

namespace Miniblog.Core.Framework.Localization
{
	public interface ITranslationProvider
	{
		/// <summary>
		/// Translates to a target language
		/// </summary>
		/// <param name="key">Path to the translated object "." separetes the hirachy</param>
		/// <param name="args">Optional parameters</param>
		/// <returns></returns>
		string Translate(string key, params object[] args);

		/// <summary>
		/// Translates to a target language
		/// </summary>
		/// <param name="culture"></param>
		/// <param name="key">Path to the translated object "." separetes the hirachy</param>
		/// <param name="args">Optional parameters</param>
		/// <returns></returns>
		string Translate(CultureInfo culture, string key, params object[] args);
	}
}