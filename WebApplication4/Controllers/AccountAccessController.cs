using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class AccountAccessController 
    {
		NpgsqlConnection connection;
		public AccountAccessController(NpgsqlConnection connection)
		{
			this.connection = connection;
		}
		public void MakeAccount(string accountname, string type)
		{
			var count = connection.ExecuteScalar<int?>(@"SELECT count(*) FROM account AS a WHERE a.account_name = @accountname",
								new { accountname });
			Console.WriteLine(count);	
			if (count == 0)
			{
				connection.Insert(new { accountname, type });
			}
		}

		public int GetAccountId(string accountname)
		{
			//var accountId = connection.Get<Account>(a.AccountName);
			var accountId = connection.Query<int>(@"SELECT a.account_id
                                                FROM account AS a 
                                                WHERE a.account_name = @accountname", new { accountname }).FirstOrDefault();
			return accountId;

		}
		public IEnumerable<Account> GetAccountLists()
		{
				IEnumerable<Account>? list = connection.GetAll<Account>();
				return list;

		}

	}
}
