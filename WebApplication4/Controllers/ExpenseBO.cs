using Dapper;
using Npgsql;
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
		private int TransactionValidations(MakeExpensePayload payload)
		{
			int flag = 0;
			var validationMessages = new List<string>();

			if (payload.Amount <= 0)
			{
				validationMessages.Add("Amount cannot be negative");
				Console.WriteLine("Amount cannot be negative");
			}

			if (string.IsNullOrEmpty(payload.Note))
			{
				validationMessages.Add("Note cannot be empty");
				Console.WriteLine("Note cannot be empty");
			}

			if (string.IsNullOrEmpty(payload.AccountName))
			{
				validationMessages.Add("Please choose an Account");
				Console.WriteLine("Please choose an Account");
			}
/*            if (!string.IsNullOrEmpty(payload.AccountName) && connection.QueryFirst<int>(@"select * FROM account AS a INNER JOIN transaction AS t ON a.account_id = t.account_id
													where account_name = @accountname and date=@date",
                                        new { payload.AccountName, payload.Date }) > 1)
            {
                validationMessages.Add($"A transaction with account name:- {payload.AccountName} and Date:- {payload.Date} already exists");
            }*/
            if (validationMessages.Any())
			{
				flag = 1;
				return flag;
			}
			else
			{
				flag = 0;
				return flag;
			}

		}


		private int AccountValidations(CreateAccountPayload payload)
		{
			int flag = 0;
			var validationMessages = new List<string>();

			if (string.IsNullOrEmpty(payload.AccountName))
			{
				validationMessages.Add("AccountName cannot be empty");
				Console.WriteLine("AccountName cannot be empty");

			}
			if (connection.QueryFirst<int>(@"SELECT count(*) FROM account AS a WHERE a.account_name = @accountname",
				new { payload.AccountName }) > 0)
			{
				validationMessages.Add("Account exists with same name");
				Console.WriteLine("Account exists with same name");
			}
			if (string.IsNullOrEmpty(payload.Type))
			{
				validationMessages.Add("Type cannot be empty");
				Console.WriteLine("Type cannot be empty");
			}
			if (!string.IsNullOrEmpty(payload.Type) && (payload.Type != "income" | payload.Type != "expense"))
			{
				validationMessages.Add("Type should be wither income or expense ");
				Console.WriteLine("Type should be wither income or expense ");
			}
			if (validationMessages.Any())
			{
				flag = 1;
				return flag;
			}
			else
			{
				flag = 0;
				return flag;
			}
		}

		public void MakeExpense(MakeExpensePayload payload)
		{
			if (TransactionValidations(payload) == 0)
			{
				Console.WriteLine("called MakeExpense");
				var transactionAccess = new TransactionAccessController(connection);
				var accountAccess = new AccountAccessController(connection);
				var accountId = accountAccess.GetAccountId(payload.AccountName);
				Console.WriteLine(accountId);
				payload.Account_Id = accountId;
				transactionAccess.MakeTransaction(payload);
			}
		}
		public void CreateAccount(CreateAccountPayload payload)
		{
			if (AccountValidations(payload) == 0)
			{
				var accountAccess = new AccountAccessController(connection);
				accountAccess.MakeAccount(payload.AccountName, payload.Type);
			}
		}
	}
}
