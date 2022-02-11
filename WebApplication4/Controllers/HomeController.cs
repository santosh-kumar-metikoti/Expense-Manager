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

        NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Username=postgres;Password=1234;Database=ExpenseManagerDB");
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
<<<<<<< HEAD
                AccountAccessController accountAccess = new AccountAccessController(connection);
                TransactionAccessController transactionAccess = new TransactionAccessController(connection);
                IEnumerable<Account> acc = accountAccess.GetAccountLists();
                return View(transactionAccess.GetTransactions());
        }

        public void AddNewAccount(string account, string type)
        {
            var payl = new ExpensePayload
            {
                Account = new Account { AccountName = account, Type = type }
            };

            var bo = new ExpenseBO(connection);
            bo.CreateAccount(payl);
        }
=======
            try
            {
                _logger.LogInformation($"{DateTime.Now} HomePage");
                ViewBag.accounts = GetAccountLists().OrderByDescending(x => x.AccountName).ToList();
                _logger.LogInformation($"{DateTime.Now} Got {ViewBag.accounts.Count} accounts");
                using (var connection = new NpgsqlConnection(connString))
                {
                    var model = new TransactionViewModel();
                    string query = @"SELECT t.transaction_id, a.account_name, a.type, t.amount, t.date
                                                               FROM account AS a
                                                               INNER JOIN transaction AS t ON a.account_id = t.account_id";
                    model.Transactions = connection.Query<Transaction>(query);
                    _logger.LogInformation($"{DateTime.Now} Retrieved {model.Transactions.Count()} Transactions");

                    return View(model);
                }
            }
            catch (Exception x)
            {
                _logger.LogError($"The query failed with {x.Message}");
                throw;
            }
        } 
>>>>>>> 51aeccbeca6508a0311a5f8cdbe327910d377827

        [HttpPost]
        public IActionResult AddTransaction(string account, int amount, DateTime date, string note)
        {
<<<<<<< HEAD
            var payl = new ExpensePayload
            {
                Transaction = new Transaction { Amount = amount, Date = date, Note = note },
                Account = new Account { AccountName = account}
            };

            var bo = new ExpenseBO(connection);
            bo.MakeExpense(payl);
            return View(nameof(AddTransaction));
=======
            try
            {
                _logger.LogInformation($"{DateTime.Now} HomePage");
                using (NpgsqlConnection connection = new NpgsqlConnection(connString))
                {
                    var query = connection.Execute(@"INSERT INTO transaction(account_id,amount,date,note)
                                                    SELECT a.account_id,@amount, @date, @note
                                                    FROM account AS a
                                                    WHERE a.account_name=@account", new { amount, date, note, account });
                    _logger.LogInformation($"Inserted {account},{amount},{date},{note} into new Transaction");
                    if (query > 0)
                    {
                        return View(nameof(AddTransaction));
                    }
                }
            }
            catch (Exception x)
            {
                _logger.LogError($"The query failed with {x.Message}");
                throw;
            }
            return View();
        }

        public IActionResult AddNewAccount(string account, string type)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now} Add Account Page");
                using (NpgsqlConnection connection = new NpgsqlConnection(connString))
                {
                    var count = connection.ExecuteScalar<int?>(@"SELECT * FROM account AS a WHERE a.account_name = @account",
                                                    new { account });
                    _logger.LogInformation($"Executing query to check Whether Account Exists with Name {account}");
                    if (count == 0)
                    {
                        connection.Execute(@"INSERT INTO account(account_name, type)
                                        VALUES(@account, @type)", new { account, type });
                        _logger.LogInformation($"Account Doesn't exists with name {account} so adding a account named {account}");
                        return View(nameof(AddedView));
                    }
                    return View(Error());
                }
            }
            catch (Exception x)
            {
                _logger.LogError($"The query failed with {x.Message}");
                throw;
            }
>>>>>>> 51aeccbeca6508a0311a5f8cdbe327910d377827
        }


        public IActionResult TransactionInfo(int id)
        {
<<<<<<< HEAD
            var TransactionAccessController = new TransactionAccessController(connection);
            return View(TransactionAccessController.GetTransactionInfo(id));
=======
            try
            {
                _logger.LogInformation($"{DateTime.Now} Transaction Info Page");
                using (NpgsqlConnection connection = new NpgsqlConnection(connString))
                {
                    var model = new TransactionInfoViewModel();
                    string query = @"SELECT a.account_name, a.type, DATE(t.date), t.amount, t.note, t.transaction_id 
                                                                                    FROM transaction AS t 
                                                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                                    WHERE t.transaction_id = @id";
                    model.TransactionsInfo = connection.Query<Transaction>(query, new { id });
                    _logger.LogInformation($"{DateTime.Now} Transaction Information With id-- {id}");
                    return View(model);
                }
            }
            catch (Exception x)
            {
                _logger.LogError($"{DateTime.Now} The query failed with {x.Message}");
                throw;
            }
>>>>>>> 51aeccbeca6508a0311a5f8cdbe327910d377827
        }

        /*Filtering Transactions between startDate and endDate and caluculating Income and Expense*/
        public IActionResult AllTransactionsList(DateTime startDate, DateTime endDate)
        {
<<<<<<< HEAD
            var  allTransactionViewModel = new TransactionAccessController(connection);
            var model = new AllTransactionViewModel();
            model.AllTransactions = allTransactionViewModel.GetFilteredTransactionsList(startDate, endDate);
            model.IncomeLists = allTransactionViewModel.GetIncome(startDate, endDate);
            model.ExpenseLists = allTransactionViewModel.GetExpense(startDate, endDate);
            return View(model);
=======
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connString))
                {
                    var model = new AllTransactionViewModel();
                    model.AllTransactions = connection.Query<Transaction>(@"SELECT a.account_name, a.type, DATE(t.date), t.transaction_id, t.amount, t.note 
                                                                                    FROM transaction AS t 
                                                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                                    WHERE t.date BETWEEN @startDate AND @endDate 
                                                                                    ORDER BY t.date",
                                                                                        new { startDate, endDate });
                    _logger.LogInformation($"{DateTime.Now} List of Transactions made between {startDate} and {endDate}");
                    model.IncomeLists = connection.Query<Transaction>(@"SELECT SUM(t.amount) as Expense
                                                                            FROM transaction AS t
                                                                            INNER JOIN account AS a ON t.account_id = a.account_id 
                                                                            WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'expense'",
                                                                                 new { startDate, endDate });

                    _logger.LogInformation($"caluculated expense between {startDate} {endDate}");

                    model.ExpenseLists = connection.Query<Transaction>(@"SELECT SUM(t.amount) as Income 
                                                    FROM transaction AS t 
                                                    INNER JOIN account AS a ON t.account_id = a.account_id 
                                                    WHERE t.date BETWEEN @startDate AND @endDate AND a.type = 'income' group by t.account_id",
                                                        new { startDate, endDate });
                    _logger.LogInformation($"{DateTime.Now} caluculated income between {startDate} {endDate}");
                    return View(model);

                }
            }
            catch (Exception x)
            {
                _logger.LogError($"{DateTime.Now} The query failed with {x.Message}");
                throw;
            }

>>>>>>> 51aeccbeca6508a0311a5f8cdbe327910d377827
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
<<<<<<< HEAD
=======
        public IEnumerable<Account_list> GetAccountLists()
        {
            try
            {
                using var connection = new NpgsqlConnection(connString);

                IEnumerable<Account_list>? list = connection.Query<Account_list>(@"SELECT a.account_id as AccountID,a.account_name as AccountName
                                                                             FROM account AS a");
                _logger.LogInformation($"{DateTime.Now} Fetching accounts");
                return list;
            }
            catch (Exception x)
            {
                _logger.LogError($" {DateTime.Now} Failed to fetch accounts with error {x.Message}");
                throw;
            }
        }
>>>>>>> 51aeccbeca6508a0311a5f8cdbe327910d377827
    }
}