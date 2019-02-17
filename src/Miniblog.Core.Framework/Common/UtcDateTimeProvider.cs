using System;

namespace Miniblog.Core.Framework.Common
{
	public class UtcDateTimeProvider : IDateTimeProvider
	{
		public UtcDateTimeProvider()
		{
			DateTimeValue = DateTime.UtcNow;
		}
		public DateTime DateTimeValue { get; private set; }
	}
}