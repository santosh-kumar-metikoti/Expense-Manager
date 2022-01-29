using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication4.Models;
using Npgsql;
using System.Data;
using NpgsqlTypes;
using Dapper;
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
            ViewBag.accounts = GetAccountLists().OrderByDescending(x => x.AccountName).ToList();

            List<Transaction> transactionLists = new List<Transaction>();

             using (var connection = new NpgsqlConnection(connString))
             {
                 List<Transaction>? transactions = connection.Query<Transaction>(@"SELECT t.transaction_id, a.account_name, a.type, t.amount, t.date
                                                                                  FROM account AS a
                                                                                  INNER JOIN transaction AS t ON a.account_id = t.account_id").ToList();
                 transactionLists.AddRange(transactions);
             }
            var model = new TransactionViewModel();
            model.Transactions = transactionLists;

            return View(model);
        } 

        [HttpPost]
        public IActionResult AddTransaction(string account, int amount, DateTime date, string note)
        {

                using (NpgsqlConnection connection = new NpgsqlConnection(connString))
                {
                    var query = connection.Execute(@"INSERT INTO transaction(account_id,amount,date,note)
                                                    SELECT a.account_id,@amount, @date, @note
                                                    FROM account AS a
                                                    WHERE a.account_name=@account", new {amount, date, note, account});
                    if (query > 0)
                    {
                        return View(nameof(AddTransaction));
                    }
                }
            return View();
        }

        public IActionResult AddNewAccount(string account, string type)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                var count = connection.ExecuteScalar<int>(@"SELECT * FROM account AS a WHERE a.account_name = @account",
                                                new { account});
                if (count == 0)
                {
                    connection.Execute(@"INSERT INTO account(account_name, type)
                                        VALUES(@account, @type)", new { account,type });
                    return View(nameof(AddedView));
                }
            }
            return View(nameof(AddedView));
        }

        public IActionResult TransactionInfo(int id)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                List<Transaction> transactionInfoLists = new List<Transaction>();

                List<Transaction>? transactionsInfo = connection.Query<Transaction>(@"SELECT a.account_name, a.type, DATE(t.date), t.amount, t.note, t.transaction_id 
                                                                                    FROM transaction AS t 
                                                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                                    WHERE t.transaction_id = @id", new {id}).ToList();
                transactionInfoLists.AddRange(transactionsInfo);

                var model = new TransactionInfoViewModel();
                model.TransactionsInfo = transactionInfoLists;
                return View(model);
            }
        }


        /*Filtering Transactions between startDate and endDate and caluculating Income and Expense*/
        public IActionResult AllTransactionsList(DateTime startDate, DateTime endDate)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                List<Transaction> allTransactionLists = new List<Transaction>();

                List<Transaction> incomeLists = new List<Transaction>();
                List<Transaction> expenseLists = new List<Transaction>();

                List<Transaction>? transactions = connection.Query<Transaction>(@"SELECT a.account_name, a.type, DATE(t.date), t.transaction_id, t.amount, t.note 
                                                                                    FROM transaction AS t 
                                                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                                    WHERE t.date BETWEEN @startDate AND @endDate 
                                                                                    ORDER BY t.date",
                                                                                    new { startDate, endDate }).ToList();
                 allTransactionLists.AddRange(transactions);

                List<Transaction>? expense = connection.Query<Transaction>(@"SELECT SUM(t.amount) as Expense
                                                                            FROM transaction AS t
                                                                            INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                            WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'expense'", 
                                                                             new { startDate, endDate }).ToList();
                incomeLists.AddRange(expense);


                List<Transaction>? income = connection.Query<Transaction>(@"SELECT SUM(t.amount) as Income 
                                                    FROM transaction AS t 
                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                    WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'income' group by t.account_id",
                                                    new { startDate, endDate }).ToList();
                expenseLists.AddRange(income);

                var model = new AllTransactionViewModel();
                model.AllTransactions = allTransactionLists;
                model.IncomeLists = incomeLists;
                model.ExpenseLists = expenseLists;

                return View(model);

            }

        }

        public ViewResult AllTransactions()
        {
            return View();
        }

        public ViewResult AddedView()
        {
            return View();
        }
        public ViewResult AddAccount()
        {
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

            List<Account_list> account_Lists = new List<Account_list>();

            List<Account_list>? list = connection.Query<Account_list>(@"SELECT a.account_id as AccountID,a.account_name as AccountName
                                                                        FROM account AS a").ToList();
            account_Lists.AddRange(list);

            return account_Lists;
        }

    }
}