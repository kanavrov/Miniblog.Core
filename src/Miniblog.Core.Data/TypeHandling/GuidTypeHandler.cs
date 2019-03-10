using System;
using static Dapper.SqlMapper;

namespace Miniblog.Core.Data.TypeHandling
{
	public class GuidTypeHandler : TypeHandler<Guid>
	{
		public override Guid Parse(object value)
		{
			if (value is byte[] byteArray)
			{
				return new Guid(byteArray);
			}

			return new Guid(value.ToString());
		}

		public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
		{
			parameter.Value = value.ToString();
		}
	}
}