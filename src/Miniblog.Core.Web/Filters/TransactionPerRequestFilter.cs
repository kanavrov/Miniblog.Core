using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Miniblog.Core.Web.Filters
{
    public class TransactionPerRequestFilter : IAsyncActionFilter
    {
        private readonly IDbTransaction transaction;

        public TransactionPerRequestFilter(IDbTransaction transaction)
        {
            this.transaction = transaction;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var connection = transaction.Connection;
            if (connection.State != ConnectionState.Open)
                throw new NotSupportedException("Database connection is not opened.");

            var executedContext = await next.Invoke();

            if (executedContext.Exception == null)
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }
        }
    }
}