using System.Collections.Generic;
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
		/// <param name="culture">Target culture.</param>
		/// <param name="key">Path to the translated object "." separetes the hirachy</param>
		/// <param name="args">Optional parameters</param>
		/// <returns></returns>
		string Translate(CultureInfo culture, string key, params object[] args);

		/// <summary>
		/// Get all translations with key prefix for current culture.
		/// </summary>
		/// <param name="culture">Target culture.</param>
		/// <param name="prefix">Target key prefix.</param>
		/// <returns>A translation model containing all translations.</returns>
		ITranslationModel GetAllTranslations(string prefix = null);

		/// <summary>
		/// Get all translations with key prefix for target culture.
		/// </summary>
		/// <param name="culture">Target culture.</param>
		/// <param name="prefix">Target key prefix.</param>
		/// <returns>A translation model containing all translations.</returns>
		ITranslationModel GetAllTranslations(CultureInfo culture, string prefix = null);
	}
}