using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Miniblog.Core.Framework.Data;
using Miniblog.Core.Web.Extensions;

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
			var noTransactionAttribute = context.GetActionAttribute<NoTransactionAttribute>();
            
			if(noTransactionAttribute != null) 
			{
				await next.Invoke();
				return;
			}

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