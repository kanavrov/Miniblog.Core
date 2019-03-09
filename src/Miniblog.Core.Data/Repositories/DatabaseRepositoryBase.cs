using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Miniblog.Core.Data.Models;

namespace Miniblog.Core.Data.Repositories
{
	public abstract class DatabaseRepositoryBase 
	{
		protected readonly IDbTransaction _transaction;
		protected readonly IDbConnection _connection;

		protected DatabaseRepositoryBase(IDbTransaction transaction)
		{
			_transaction = transaction;
			_connection = transaction.Connection;
		}

		protected async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string command, object param = null)
        {
            return await _connection.QueryAsync<T>(command, param, _transaction);
        }

		protected async Task<T> ExecuteQueryFirstOrDefaultAsync<T>(string command, object param = null)
        {
            return await _connection.QueryFirstOrDefaultAsync<T>(command, param, _transaction);
        }

		protected async Task<T> ExecuteScalarAsync<T>(string command, object param = null)
        {
            return await _connection.ExecuteScalarAsync<T>(command, param, _transaction);
        }

		protected async Task ExecuteNonQueryAsync(string command, object param = null)
        {
            await _connection.ExecuteAsync(command, param, _transaction);
        }
	}
}