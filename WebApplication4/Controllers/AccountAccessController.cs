using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class AccountAccessController : Controller
    {
		NpgsqlConnection connection;
		public AccountAccessController(NpgsqlConnection connection)
		{
			this.connection = connection;
		}
		public void MakeAccount(Account a)
		{
			var count = connection.ExecuteScalar<int?>(@"SELECT count(*) FROM account AS a WHERE a.account_name = @accountname",
								new { a.AccountName });
			Console.WriteLine(count);	
			if (count == 0)
			{
				connection.Execute(@"INSERT INTO account(account_name, type)
                             	VALUES(@accountname, @type)", new { a.AccountName, a.Type });
				//return View(nameof(AddedView));
			}
		}

		public int GetAccountId(Account a)
		{
			var accountId = connection.Query<int>(@"SELECT a.account_id
                                                FROM account AS a 
                                                WHERE a.account_name = @accountname", new { a.AccountName }).FirstOrDefault();
			return accountId;

		}
		public IEnumerable<Account> GetAccountLists()
		{

				IEnumerable<Account>? list = connection.Query<Account>(@"SELECT a.account_id as AccountID,a.account_name as AccountName
                                                                             FROM account AS a");
				return list;

		}

	}
}
