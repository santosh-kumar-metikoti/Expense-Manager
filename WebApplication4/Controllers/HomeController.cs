using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication4.Models;
using Npgsql;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using NpgsqlTypes;

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
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var command = new NpgsqlCommand(null, conn);
            command.CommandText =
                "SELECT t.transaction_id, a.account_name, a.type,t.amount,t.date"+
                " FROM account AS a INNER JOIN transaction AS t "+
                " ON a.account_id = t.account_id";
            command.Prepare();
            NpgsqlDataReader reader = command.ExecuteReader();

            List<Transaction> transactionLists = new List<Transaction>();
            

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

                using (NpgsqlConnection connection = new NpgsqlConnection(connString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(null, connection);

                    command.CommandText =
                        "INSERT INTO transaction(account_id,amount,date,note)" +
                        "SELECT a.account_id,@amount, @date, @note "+
                        "FROM account AS a "+
                        "WHERE a.account_name=@account";
                    NpgsqlParameter amountParam = new NpgsqlParameter("@amount", NpgsqlDbType.Bigint, 0);
                    NpgsqlParameter noteParam = new NpgsqlParameter("@note", NpgsqlDbType.Varchar, 100);
                    NpgsqlParameter dateParam = new NpgsqlParameter("@date", NpgsqlDbType.Date, 0);
                    NpgsqlParameter accountParam = new NpgsqlParameter("@account", NpgsqlDbType.Varchar, 0);

                    amountParam.Value = amount;
                    noteParam.Value = note;
                    dateParam.Value = date;
                    accountParam.Value = account;

                    command.Parameters.Add(amountParam);
                    command.Parameters.Add(noteParam);
                    command.Parameters.Add(dateParam);
                    command.Parameters.Add(accountParam);


                    command.Prepare();
                    int result = command.ExecuteNonQuery();
                    if (result < 0)
                    {
                        return Error();
                    }
                    return View(nameof(AddTransaction));
                }

            }
            return View();

        }
        public IActionResult AddNewAccount(string account, string type)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(null, connection);

                command.CommandText =
                    "SELECT * FROM account AS a WHERE a.account_name = @account";
                NpgsqlParameter accountParam = new NpgsqlParameter("@account", NpgsqlDbType.Varchar, 0);

                accountParam.Value = account;

                command.Parameters.Add(accountParam);


                command.Prepare();
                command.ExecuteNonQuery();

                var dataset = new DataSet();
                var dataAdpter = new NpgsqlDataAdapter(command);
                dataAdpter.Fill(dataset);

                if (dataset.Tables[0].Rows.Count > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                else
                {
                        NpgsqlCommand command2 = new NpgsqlCommand(null, connection);

                        command.CommandText =
                            "INSERT INTO account(account_name,type)"+
                            "VALUES(@accountName, @type)";
                        NpgsqlParameter accountNameParam = new NpgsqlParameter("@accountName", NpgsqlDbType.Varchar, 100);
                        NpgsqlParameter typeParam = new NpgsqlParameter("@type", NpgsqlDbType.Varchar, 0);

                        typeParam.Value = type;
                        accountNameParam.Value = account;

                    command.Parameters.Add(accountNameParam);
                    command.Parameters.Add(typeParam);


                        command.Prepare();
                        int result = command.ExecuteNonQuery();
                        if (result < 0)
                        {
                            return Error();
                        }
                    return View(nameof(AddedView));
                }
            }
        }

        public IActionResult TransactionInfo(int id) 
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(null, connection);

                command.CommandText =
                    "SELECT a.account_name,a.type,DATE(t.date),t.amount,t.note, t.transaction_id "+
                    "FROM transaction AS t "+
                    "INNER JOIN account AS a ON t.account_id = a.account_id "+
                    "WHERE t.transaction_id = @id";
                NpgsqlParameter idParam = new NpgsqlParameter("@id", NpgsqlDbType.Integer, 0);

                idParam.Value = id;

                command.Parameters.Add(idParam);


                command.Prepare();
                NpgsqlDataReader reader = command.ExecuteReader();

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
                var model = new TransactionInfoViewModel();
                model.TransactionsInfo = transactionInfoLists;
                return View(model);
            }
        }

        public IActionResult AllTransactionsList(DateTime startDate, DateTime endDate)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {



                //-----------------
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(null, connection);

                command.CommandText =
                    "SELECT a.account_name,a.type, DATE(t.date),t.transaction_id,t.amount,t.note "+
                    "FROM transaction AS t "+
                    "INNER JOIN account AS a ON t.account_id = a.account_id "+
                    "WHERE t.date BETWEEN @startDate AND @endDate "+
                    "ORDER BY t.date";
                /*               command.CommandText =
                                     "SELECT SUM(t.amount) as income,a.type FROM transaction AS t INNER JOIN account AS a ON t.account_id = a.account_id WHERE a.type = 'income' AND t.date BETWEEN @startDate and @endDate GROUP BY a.type";*/
                /* command.CommandText =
                   "SELECT SUM(t.amount) as expense,a.type FROM transaction AS t INNER JOIN account AS a ON t.account_id = a.account_id WHERE a.type = 'expense' AND t.date BETWEEN @startDate and @endDate GROUP BY a.type";*/
                NpgsqlParameter startDateParam = new NpgsqlParameter("@startDate", NpgsqlDbType.Date, 0);
                NpgsqlParameter endDateParam = new NpgsqlParameter("@endDate", NpgsqlDbType.Date, 0);

                startDateParam.Value = startDate;
                endDateParam.Value = endDate;

                command.Parameters.Add(startDateParam);
                command.Parameters.Add(endDateParam);

                command.Prepare();
                NpgsqlDataReader reader = command.ExecuteReader();

                List<Transaction> allTransactionLists = new List<Transaction>();

                List<Transaction> incomeLists = new List<Transaction>();
                List<Transaction> expenseLists = new List<Transaction>();

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
                connection.Close();
                reader.Close();

                var model = new AllTransactionViewModel();
                model.AllTransactions = allTransactionLists;
                model.IncomeLists = incomeLists;
                model.ExpenseLists = expenseLists;

                return View(model);
            }




            //--------------------------------------------------------------------
            /*var dataset = new DataSet();
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            using var connection1 = new NpgsqlConnection(connString);
            connection1.Open();
            using var connection2 = new NpgsqlConnection(connString);
            connection2.Open();*//*
            Console.WriteLine(startDate);
            *//*query to account name, type, date, amount note from transactions and account table based on transaction_id*//*
            var query = String.Format(@"SELECT a.account_name,a.type, DATE(t.date),t.transaction_id,t.amount,t.note 
                                          FROM transaction AS t
                                               INNER JOIN account AS a
                                               ON t.account_id = a.account_id
                                         WHERE t.date BETWEEN '{0}' 
                                           AND '{1}' ORDER BY t.date;", startDate, endDate);

            *//*query to caluculate sum of all rows in amount column from transaction table* of tyoe="income"*//*
            var incomeQuery = String.Format(@"SELECT SUM(t.amount) as income,a.type 
                                                FROM transaction AS t
                                                     INNER JOIN account AS a
                                                     ON t.account_id = a.account_id 
                                               WHERE a.type='income'
                                                 AND t.date BETWEEN '{0}' and '{1}' 
                                            GROUP BY a.type", startDate, endDate);

            *//*query to caluculate sum of all rows in amount column from transaction table tyoe="expense"*//*
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

*//*            using var incomeSqlCommand = new NpgsqlCommand(incomeQuery, connection1);
            NpgsqlDataReader reader2 = incomeSqlCommand.ExecuteReader();*//*

            List<Transaction> incomeLists = new List<Transaction>();



*//*            using var expenseSqlCommand = new NpgsqlCommand(expenseQuery, connection2);
            NpgsqlDataReader reader3 = expenseSqlCommand.ExecuteReader();*//*

            List<Transaction> expenseLists = new List<Transaction>();
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
           *//* if (reader2.HasRows)

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

            }*//*

            var model = new AllTransactionViewModel();
            model.AllTransactions = allTransactionLists;
            model.IncomeLists = incomeLists;
            model.ExpenseLists = expenseLists;

            return View(model);*/

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