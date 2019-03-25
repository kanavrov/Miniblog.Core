using System;
using System.Runtime.Serialization;

namespace Miniblog.Core.Service.Exceptions
{
	public class FilePersistException : Exception
	{
		public FilePersistException()
		{
		}

		public FilePersistException(string message) : base(message)
		{
		}

		public FilePersistException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected FilePersistException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}