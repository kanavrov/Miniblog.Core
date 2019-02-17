using System;

namespace Miniblog.Core.Framework.Common
{
	public interface IDateTimeProvider
    {
         DateTime DateTimeValue { get; }
    }
}