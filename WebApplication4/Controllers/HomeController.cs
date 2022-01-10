using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication4.Models;
using Npgsql;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq; 

namespace WebApplication4.Controllers
{ 
    public class HomeController : Controller  
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string connString = "Host=localhost;Username=postgres;Password=1234;Database=ExpenseManagerDB";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
           // var dataset = new DataSet();
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            
            /* getting account name, type, date, amount from transaction and account table*/
            var sql = @"SELECT t.transaction_id, a.account_name, a.type,t.amount,t.date
                          FROM account as a
                                 INNER JOIN transaction AS t
                                 ON a.account_id = t.account_id";

            using var sqlCommand = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader reader = sqlCommand.ExecuteReader();
            List<Transaction> transactionLists = new List<Transaction>();
            
            /* using (var command = new NpgsqlCommand(query, connection))
            {
                var adapter = new NpgsqlDataAdapter(command);
                adapter.Fill(dataset);
            }*/

            if (reader.HasRows)

            {

                while (reader.Read())

                {

                    transactionLists.Add(new Transaction

                    {

                        TransactionId = Convert.ToInt32(reader["transaction_id"]),

                        AccountName = Convert.ToString(reader["account_name"]),

                        Type = Convert.ToString(reader["type"]),

                        Date = Convert.ToDateTime(reader["date"]),

                        Amount = Convert.ToInt32(reader["amount"]),


                    });

                }

            }

            ViewBag.accounts = GetAccountLists().OrderByDescending( x=>x.AccountName).ToList();

            var model = new TransactionViewModel();
            model.Transactions = transactionLists;

            return View(model);
        }

        [HttpPost]
        public IActionResult AddTransaction(string account, int amount, DateTime date, string note)
        {
            if (ModelState.IsValid)
            {
                using var connection = new NpgsqlConnection(connString);
                connection.Open();
                /*inserting transaction details into transaction table*/
                string query = String.Format(@"INSERT INTO transaction(account_id,amount,date,note)
                                               SELECT a.account_id,{0},'{1}','{2}' 
                                                 FROM account a
                                                WHERE a.account_name='{3}'", amount, date, note, account);
                using var command = new NpgsqlCommand(query, connection);
                int result = command.ExecuteNonQuery();
                if (result < 0)
                {
                    return Error();
                }
                return View(nameof(AddTransaction));
            }
            return View();

        }
        public IActionResult AddNewAccount(string account, string type)
        {
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            /*query to account table satisfying specific condition*/
            string sql = String.Format(@"SELECT * FROM account AS a
                                         WHERE a.account_name='{0}'", account);
            using var sqlCommand = new NpgsqlCommand(sql, connection);
            var dataset = new DataSet();
            var dataAdpter = new NpgsqlDataAdapter(sqlCommand);
            dataAdpter.Fill(dataset);

            if (dataset.Tables[0].Rows.Count > 0)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                string query = String.Format(@"INSERT INTO account(account_name,type) 
                                               VALUES ('{0}','{1}');", account, type);
                using var command = new NpgsqlCommand(query, connection);
                int result = command.ExecuteNonQuery();
                if (result < 0)
                {
                    return Error();
                }
                return View(nameof(AddedView));
            }

        }

        public IActionResult TransactionInfo(int id) 
        {
            //var dataset = new DataSet();
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            /*query to account name, type, date, amount note from transactions and account table based on transaction_id*/
            var sql = String.Format(@"SELECT a.account_name,a.type,DATE(t.date),t.amount,t.note, t.transaction_id
                                        FROM transaction AS t
                                                INNER JOIN account AS a
                                                ON t.account_id = a.account_id 
                                       WHERE t.transaction_id={0}", id);
            using var sqlCommand = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader reader = sqlCommand.ExecuteReader();
            List<Transaction> transactionInfoLists = new List<Transaction>();
            if (reader.HasRows)
            {

                while (reader.Read())
                {
                    transactionInfoLists.Add(new Transaction
                    {

                        AccountName = Convert.ToString(reader["account_name"]),

                        Date = Convert.ToDateTime(reader["date"]),

                        Type = Convert.ToString(reader["type"]),

                        Amount = Convert.ToInt32(reader["amount"]),

                        Note = Convert.ToString(reader["note"])

                    });

                }

            }
            /*  using (var command = new NpgsqlCommand(sql, connection))
            {
                var adapter = new NpgsqlDataAdapter(command);
                adapter.Fill(dataset);
            }*/
            var model = new TransactionInfoViewModel();
            model.TransactionsInfo = transactionInfoLists;
            return View(model);
        }

        public IActionResult AllTransactionsList(DateTime startDate, DateTime endDate)
        {
            var dataset = new DataSet();
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            using var connection1 = new NpgsqlConnection(connString);
            connection1.Open();
            using var connection2 = new NpgsqlConnection(connString);
            connection2.Open();
            Console.WriteLine(startDate);
            /*query to account name, type, date, amount note from transactions and account table based on transaction_id*/
            var query = String.Format(@"SELECT a.account_name,a.type, DATE(t.date),t.transaction_id,t.amount,t.note 
                                          FROM transaction AS t
                                               INNER JOIN account AS a
                                               ON t.account_id = a.account_id
                                         WHERE t.date BETWEEN '{0}' 
                                           AND '{1}' ORDER BY t.date;", startDate, endDate);

            /*query to caluculate sum of all rows in amount column from transaction table* of tyoe="income"*/
            var incomeQuery = String.Format(@"SELECT SUM(t.amount) as income,a.type 
                                                FROM transaction AS t
                                                     INNER JOIN account AS a
                                                     ON t.account_id = a.account_id 
                                               WHERE a.type='income'
                                                 AND t.date BETWEEN '{0}' and '{1}' 
                                            GROUP BY a.type", startDate, endDate);

            /*query to caluculate sum of all rows in amount column from transaction table tyoe="expense"*/
            var expenseQuery = String.Format(@"SELECT SUM(t.amount) AS expense,a.type
                                                 FROM transaction AS t
                                                      INNER JOIN account AS a
                                                      ON t.account_id = a.account_id
                                                WHERE a.type='expense'
                                                  AND t.date BETWEEN '{0}' and '{1}'
                                             GROUP BY a.type", startDate, endDate);


            using var sqlCommand = new NpgsqlCommand(query, connection);
            NpgsqlDataReader reader = sqlCommand.ExecuteReader();

            List<Transaction> allTransactionLists = new List<Transaction>();

            using var incomeSqlCommand = new NpgsqlCommand(incomeQuery, connection1);
            NpgsqlDataReader reader2 = incomeSqlCommand.ExecuteReader();

            List<Transaction> incomeLists = new List<Transaction>();



            using var expenseSqlCommand = new NpgsqlCommand(expenseQuery, connection2);
            NpgsqlDataReader reader3 = expenseSqlCommand.ExecuteReader();

            List<Transaction> expenseLists = new List<Transaction>();

            /*                        using (var command = new NpgsqlCommand(query, connection))
                                    {
                                        var adapter = new NpgsqlDataAdapter(command);
                                        adapter.Fill(dataset);
                                    }

            using (var incomeCommand = new NpgsqlCommand(incomeQuery, connection))
                        {
                            var adapter = new NpgsqlDataAdapter(incomeCommand);
                            adapter.Fill(dataset);

                        }

                        using (var expenseCommand = new NpgsqlCommand(expenseQuery, connection))
                        {
                            var adapter = new NpgsqlDataAdapter(expenseCommand);
                            adapter.Fill(dataset);

                        }*/
            if (reader.HasRows)

            {

                while (reader.Read())

                {

                    allTransactionLists.Add(new Transaction

                    {

                        TransactionId = Convert.ToInt32(reader["transaction_id"]),

                        AccountName = Convert.ToString(reader["account_name"]),

                        Type = Convert.ToString(reader["type"]),

                        Date = Convert.ToDateTime(reader["date"]),

                        Amount = Convert.ToInt32(reader["amount"]),

                    });

                }

            }
            // to calculate income
            if (reader2.HasRows)

            {

                while (reader2.Read())

                {

                    incomeLists.Add(new Transaction

                    {

                        Income = Convert.ToInt32(reader2["income"]),

                    });

                }

            }

            // To calculate expense
            if (reader3.HasRows)

            {

                while (reader3.Read())

                {

                    expenseLists.Add(new Transaction

                    {

                        Expense = Convert.ToInt32(reader3["expense"]),

                    });

                }

            }

            var model = new AllTransactionViewModel();
            model.AllTransactions = allTransactionLists;
            model.IncomeLists = incomeLists;
            model.ExpenseLists = expenseLists;

            return View(model);

        }

        public ViewResult AllTransactions() { 
            return View();
        }

        public ViewResult AddedView() {
            return View();
        }
        public ViewResult AddAccount() {
            return View();
        }
        public IActionResult AddTransaction()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public List<Account_list> GetAccountLists()
        {
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            string sql = String.Format(@"SELECT * FROM account");
            using var sqlCommand = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader reader = sqlCommand.ExecuteReader();
            List<Account_list> account_Lists = new List<Account_list>();

            if (reader.HasRows)

            {

                while (reader.Read())

                {

                    account_Lists.Add(new Account_list

                    {

                        AccountId = Convert.ToInt32(reader["account_id"]),

                        AccountName = Convert.ToString(reader["account_name"]),

                    });

                }

            }

            return account_Lists;   


        }


    }
}