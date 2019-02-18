using System;

namespace Miniblog.Core.Framework.Common
{
	public class UtcDateTimeProvider : IDateTimeProvider
	{
		public DateTime Now => DateTime.UtcNow;
	}
}