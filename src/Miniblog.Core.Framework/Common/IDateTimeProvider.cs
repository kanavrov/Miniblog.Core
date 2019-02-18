using System;

namespace Miniblog.Core.Framework.Common
{
	public interface IDateTimeProvider
    {
         DateTime Now { get; }
    }
}