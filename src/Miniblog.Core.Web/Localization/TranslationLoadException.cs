using System;

namespace Miniblog.Core.Web.Localization
{
	public class TranslationLoadException : Exception
	{
		public TranslationLoadException()
		{
		}

		public TranslationLoadException(string message) : base(message)
		{
		}

		public TranslationLoadException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}