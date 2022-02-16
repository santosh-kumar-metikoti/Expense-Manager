using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class ExpenseBO
	{
		NpgsqlConnection connection;
		//constructor
		public ExpenseBO(NpgsqlConnection connection)
		{
			this.connection = connection;
		}
		private void TransactionValidations(MakeExpensePayload payload)
		{
			var exceptions = new List<Exception>();
			foreach (var item in exceptions)
			{
				try
				{
					if (payload.Amount <= 0)
					{
						throw new ArgumentException();
					}

					if (string.IsNullOrEmpty(payload.Note))
					{
						throw new ArgumentException();
					}

					if (string.IsNullOrEmpty(payload.AccountName))
					{
						throw new ArgumentException();
					}

				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			if (exceptions.Count > 0)
				throw new AggregateException(
					"Encountered errors while trying to do something.",
					exceptions
				);

		}


		private void AccountValidations(CreateAccountPayload payload)
		{
			var exceptions = new List<ArgumentException>();
			try
			{
				if (connection.ExecuteScalar<int?>(@"SELECT count(*) FROM account AS a WHERE a.account_name = @accountname",
									new { payload.AccountName }) > 1)
				{
					throw new ArgumentException($"Account already exists with name {payload.AccountName}");
				}
				if (string.IsNullOrEmpty(payload.AccountName))
				{
					throw new ArgumentException($"Account Name cnnot be Empty");
				}
				if (string.IsNullOrEmpty(payload.Type))
				{
					throw new ArgumentException($"Account Type cnnot be Empty");
				}
			}
			catch (ArgumentException ex)
			{
				exceptions.Add(ex);
			}

			if (exceptions.Count > 0)
				throw new AggregateException(
					"Encountered errors: ",
					exceptions
				);
		}
		public void MakeExpense(MakeExpensePayload payload)
		{
			TransactionValidations(payload);
			var transactionAccess = new TransactionAccessController(connection);
				var accountAccess = new AccountAccessController(connection);
				//acA.MakeAccount(payload.Account);
				var accountId = accountAccess.GetAccountId(payload.AccountName);
				transactionAccess.MakeTransaction(payload);
		}
		public void CreateAccount(CreateAccountPayload payload)
        {
			AccountValidations(payload);
			var accountAccess = new AccountAccessController(connection);
			accountAccess.MakeAccount(payload.AccountName,payload.Type);
		}
	}

}
