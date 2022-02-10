<Query Kind="Program">
  <NuGetReference>Dapper</NuGetReference>
  <NuGetReference>Npgsql</NuGetReference>
  <Namespace>Npgsql</Namespace>
  <Namespace>Dapper</Namespace>
</Query>

void Main()
{
	var payl = new ExpensePayload
	{
		Transaction = new Transaction { Amount = 350, Date =  new DateTime(2022, 01, 29), Note = "Goa Trip"},
		Account = new Account { Account_Name = "sport", Type = "income"}
	};

	NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Username=postgres;Password=1234;Database=ExpenseManagerDB");
	var bo = new ExpenseBO(connection);
	bo.MakeExpense(payl);
	
	var trA = new TransactionAccess(connection);
	var acA = new AccountAccess(connection);
	trA.GetTransactions();
	trA.GetIncome(new DateTime(2022, 01, 01), new DateTime(2022, 01, 30));
	trA.GetExpense(new DateTime(2022, 01, 01), new DateTime(2022, 01, 30));
	trA.GetFilteredTransactionsList(new DateTime(2022, 01, 01), new DateTime(2022, 01, 30));
}
//------------------------------------------------------------------------------
//Bussines-Object
public class ExpenseBO
{
	NpgsqlConnection connection;
	//constructor
	public ExpenseBO(NpgsqlConnection connection)
	{
		this.connection = connection;
	}
	public void MakeExpense(ExpensePayload payload)
	{
		var trA = new TransactionAccess(connection);
		var acA = new AccountAccess(connection);
		//acA.MakeAccount(payload.Account);
		trA.MakeTransaction(payload.Transaction);
		var accountId = acA.GetAccountId(payload.Account);
		payload.Transaction.Account_Id = accountId;
		
	}																							
}

public class ExpenseBO1
{
	NpgsqlConnection connection;
	//constructor
	public ExpenseBO1(NpgsqlConnection connection)
	{
		this.connection = connection;
	}
	public void MakeExpense(ExpensePayload payload)
	{
		var trA = new TransactionAccess(connection);
		var acA = new AccountAccess(connection);
		//acA.MakeAccount(payload.Account);
		trA.MakeTransaction(payload.Transaction);
		var accountId = acA.GetAccountId(payload.Account);
		payload.Transaction.Account_Id = accountId;

	}
}
//-------------------------------------------------------------------------------
// Access-layer
public class TransactionAccess
{
	NpgsqlConnection connection;
	//constructor
	public TransactionAccess(NpgsqlConnection connection)
	{
		this.connection = connection;
	}
	//To make a Transaction
	public void MakeTransaction(Transaction t)
	{
		connection.Execute(@"INSERT INTO transaction(account_id, amount, date, note) 
		values (@account_ID, @amount, @date, @note);", new { t.Amount, t.Date, t.Note, t.Account_Id });
	}

	//To get Transactions performed
	public void GetTransactions()
	{
			var transactions = connection.Query<TransactionView>(@"SELECT t.transaction_id,t.account_id,a.account_name, a.type,t.note, t.amount, t.date
                                                               FROM account AS a
                                                               INNER JOIN transaction AS t ON a.account_id = t.account_id");
			transactions.Dump();
	}

	//To caluculate income between dates
	public void GetIncome(DateTime startDate, DateTime endDate)
	{
		var Income = connection.Query<TransactionView>(@"SELECT SUM(t.amount) as Income 
	                                                    FROM transaction AS t 
	                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
	                                                    WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'income'", new { startDate, endDate });
		Income.Dump();
	}

	//To caluculate Expense between dates
	public void GetExpense(DateTime startDate, DateTime endDate)
	{
		var Income = connection.Query<TransactionView>(@"SELECT SUM(t.amount) as Income 
	                                                    FROM transaction AS t 
	                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
	                                                    WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'expense'", new { startDate, endDate });
		Income.Dump();
	}

	public void GetFilteredTransactionsList(DateTime startDate, DateTime endDate)
	{
			var filteredTransactions = connection.Query<TransactionView>(@"SELECT a.account_name, a.account_id, a.type, DATE(t.date), t.transaction_id, t.amount, t.note 
                                                                       FROM transaction AS t 
                                                                       INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                       WHERE t.date BETWEEN @startDate AND @endDate
                                                                       ORDER BY t.date", new { startDate, endDate });
			filteredTransactions.Dump();
	}

}
public class AccountAccess
{
	NpgsqlConnection connection;
	public AccountAccess(NpgsqlConnection connection)
	{
		this.connection = connection;
	}
	public void MakeAccount(Account a)
	{
			connection.Execute(@"INSERT INTO account(account_name, type)
                             	VALUES(@account_name, @type)", new { a.Account_Name, a.Type });
	}

	public int GetAccountId(Account a)
	{
		var accountId = connection.Query<int>(@"SELECT a.account_id
                                                FROM account AS a 
                                                WHERE a.account_name = @account_name", new { a.Account_Name }).FirstOrDefault() ;
		 return accountId;
																	   
	}
}
//------------------------------------------------------------------------------
public class Transaction
{
	public int Transaction_Id { get; set; }
	public int Account_Id { get; set; }
	public decimal Amount { get; set; }
	public string Note { get; set; }
	public DateTime Date { get; set; }
}
public class Account
{
	public int Account_Id { get; set; }
	public string Account_Name { get; set; }
	public string Type { get; set; }
}

public class TransactionView
{
	public int Transaction_Id { get; set; }
	public decimal Amount { get; set; }
	public string Note { get; set; }
	public DateTime Date { get; set; }
	public int Account_Id { get; set; }
	public string Account_Name { get; set; }
	public string Type { get; set; }
	public int Income { get; set; }
	public int Expense { get; set; }
}
//--------------------------------------------
public class ExpensePayload
{
	public Transaction Transaction { get; set; }
	public Account Account { get; set; }
}