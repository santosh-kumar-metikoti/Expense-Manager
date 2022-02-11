using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class ExpenseBO : Controller
    {
		NpgsqlConnection connection;
		//constructor
		public ExpenseBO(NpgsqlConnection connection)
		{
			this.connection = connection;
		}
		public void MakeExpense(ExpensePayload payload)
		{
			var trA = new TransactionAccessController(connection);
			var acA = new AccountAccessController(connection);
			//acA.MakeAccount(payload.Account);
			var accountId = acA.GetAccountId(payload.Account);
			Console.WriteLine(accountId);
			payload.Transaction.Account_Id = accountId;
			trA.MakeTransaction(payload.Transaction);

		}
		public void CreateAccount(ExpensePayload payload)
        {
			var acA = new AccountAccessController(connection);
			acA.MakeAccount(payload.Account);
		}
	}
}
