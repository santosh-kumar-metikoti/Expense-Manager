﻿using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApplication4.Models;
namespace WebApplication4.Controllers
{
    public class TransactionAccessController
    {
		NpgsqlConnection connection;
		//constructor
		public TransactionAccessController(NpgsqlConnection connection)
		{
			this.connection = connection;
		}
		//To make a Transaction
		public void MakeTransaction(MakeExpensePayload p)
		{
			connection.Insert(new { p.Account_Id, p.Amount, p.Date, p.Note });
        }

		//To get Transactions performed
		public TransactionViewModel GetTransactions()
		{
			var model = new TransactionViewModel();
			model.Transactions = connection.GetAll<TransactionViewModel>();
			/*var query = @"SELECT t.transaction_id, a.account_name, a.type, t.amount, t.date
                                                               FROM account AS a
                                                               INNER JOIN transaction AS t ON a.account_id = t.account_id";*/
			//model.Transactions = connection.Query<TransactionViewModel>(query);
			return model;
		}

		public TransactionInfoViewModel GetTransactionInfo(int id)
		{
			var model = new TransactionInfoViewModel();
			model = connection.Get<TransactionInfoViewModel>(id);
/*			model.TransactionsInfo = connection.Query<TransactionViewModel>(@"SELECT a.account_name, a.type, DATE(t.date), t.amount, t.note, t.transaction_id 
                                                                                    FROM transaction AS t 
                                                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                                    WHERE t.transaction_id = @id", new { id });*/
			return model;
		}

		//To caluculate income between dates
		public IEnumerable<AllTransactionViewModel> GetIncome(DateTime startDate, DateTime endDate)
		{
			var model = new AllTransactionViewModel();
			model.IncomeLists = connection.Query<AllTransactionViewModel>(@"SELECT SUM(t.amount) as Income 
	                                                    FROM transaction AS t 
	                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
	                                                    WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'income'", new { startDate, endDate });

			return model.IncomeLists;
		}

		//To caluculate Expense between dates
		public IEnumerable<AllTransactionViewModel> GetExpense(DateTime startDate, DateTime endDate)
		{
			var model = new AllTransactionViewModel();
			model.ExpenseLists = connection.Query<AllTransactionViewModel>(@"SELECT SUM(t.amount) as Expense 
	                                                    FROM transaction AS t 
	                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
	                                                    WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'expense'", new { startDate, endDate });
			return model.ExpenseLists;
		}

		public IEnumerable<AllTransactionViewModel> GetFilteredTransactionsList(DateTime startDate, DateTime endDate)
		{
			var model = new AllTransactionViewModel();
			model.AllTransactions = connection.Query<AllTransactionViewModel>(@"SELECT a.account_name, a.type, DATE(t.date), t.transaction_id, t.amount, t.note 
                                                                                    FROM transaction AS t 
                                                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                                    WHERE t.date BETWEEN @startDate AND @endDate 
                                                                                    ORDER BY t.date",
																				new { startDate, endDate });
			return model.AllTransactions;
		}

	}
}
