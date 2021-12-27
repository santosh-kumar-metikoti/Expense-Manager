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

        private readonly string connString = "Host=localhost;Username=postgres;Password=1234;Database=itemsDB";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var dataset = new DataSet();
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            var query = @"SELECT accounts.account,accounts.type,DATE(transactions.date),transactions.amount, transactions.transactionid FROM accounts INNER JOIN transactions ON accounts.accountid=transactions.accountid";

            using (var command = new NpgsqlCommand(query, connection))
            {
                var adapter = new NpgsqlDataAdapter(command);
                adapter.Fill(dataset);
            }

            ViewBag.accounts = GetAccountLists().OrderByDescending( x=>x.AccName).ToList();

            return View(dataset);
        }

        [HttpPost]
        public IActionResult AddTransaction(string account, int amount, DateTime date, string note)
        {
            if (ModelState.IsValid)
            {
                using var connection = new NpgsqlConnection(connString);
                connection.Open();
                string query = String.Format(@"INSERT INTO transactions(accountid,amount,date,note) SELECT accounts.accountid,{0},'{1}','{2}' FROM accounts WHERE account='{3}'", amount, date, note, account);
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
            string sql = String.Format(@"select * from accounts where account='{0}'", account);
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
                string query = String.Format(@"INSERT INTO public.""accounts""(""account"",""type"")VALUES('{0}','{1}');", account, type);
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
            var dataset = new DataSet();
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            var query = String.Format(@"SELECT accounts.account,accounts.type,DATE(transactions.date),transactions.amount,transactions.note FROM transactions FULL JOIN accounts ON transactions.accountid=accounts.accountid WHERE transactions.transactionid={0}", id);

            using (var command = new NpgsqlCommand(query, connection))
            {
                var adapter = new NpgsqlDataAdapter(command);
                adapter.Fill(dataset);
            }

            return View(dataset);
        }


        public IActionResult AllTransactionsList(DateTime startDate, DateTime endDate)
        {
            var dataset = new DataSet();
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            Console.WriteLine(startDate);
            var query = String.Format(@"SELECT accounts.account,accounts.type, DATE(transactions.date),transactions.transactionid,transactions.amount,transactions.note 
                                        FROM transactions 
                                        FULL JOIN accounts ON transactions.accountid=accounts.accountid
                                        WHERE transactions.date BETWEEN '{0}' AND '{1}' ORDER BY transactions.date;", startDate, endDate);

           var incomeQuery = String.Format(@"SELECT SUM(amount) as income,accounts.type from transactions
                Inner JOIN accounts on transactions.accountid=accounts.accountid  where accounts.type='income' and transactions.date between '{0}' and '{1}' group by accounts.type", startDate, endDate);


            var expenseQuery = String.Format(@"SELECT SUM(amount) as expense,accounts.type from transactions
                Inner JOIN accounts on transactions.accountid=accounts.accountid  where accounts.type='expense' and transactions.date between '{0}' and '{1}' group by accounts.type", startDate, endDate);
            
            using (var command = new NpgsqlCommand(query, connection))
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

            }
            return View(dataset);

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
            string sql = String.Format(@"select * from accounts");
            using var sqlCommand = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader reader = sqlCommand.ExecuteReader();
            List<Account_list> account_Lists = new List<Account_list>();

            if (reader.HasRows)

            {

                while (reader.Read())

                {

                    account_Lists.Add(new Account_list

                    {

                        Accid = Convert.ToInt32(reader["accountid"]),

                        AccName = Convert.ToString(reader["account"]),

                    });

                }

            }

            return account_Lists;   


        }


    }
}