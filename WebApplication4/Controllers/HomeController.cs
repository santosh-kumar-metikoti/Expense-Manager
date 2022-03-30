using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication4.Models;
using Npgsql;
using System.Data;
using NpgsqlTypes;
using Dapper;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

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
            AddCsvData();
            AccountAccessController accountAccess = new AccountAccessController(connection);
            TransactionAccessController transactionAccess = new TransactionAccessController(connection);
            IEnumerable<Account> list = accountAccess.GetAccountLists();
                return View(transactionAccess.GetTransactions());
        }

        public void AddNewAccount(string account, string type)
        {
            if (account.Length! == 0 && (type.Equals("income")| type.Equals("expense")))
            {
                var payl = new CreateAccountPayload { AccountName = account, Type = type };
                var bo = new ExpenseBO(connection);
                bo.CreateAccount(payl);
            }
        }

        [HttpPost]
        public IActionResult AddTransaction(string account, int amount, DateTime date, string note)
        {
            if (account! == null && amount>0 && note! == null )
            {
                var payl = new MakeExpensePayload { Amount = amount, Date = date, Note = note, AccountName = account };

                var bo = new ExpenseBO(connection);
                bo.MakeExpense(payl);
                return View(nameof(AddTransaction));
            }
            return Error();
        }

        public IActionResult TransactionInfo(int id)
        {
            var TransactionAccessController = new TransactionAccessController(connection);
            return View(TransactionAccessController.GetTransactionInfo(id));
        }

        /*Filtering Transactions between startDate and endDate and caluculating Income and Expense*/


        public IActionResult AllTransactionsList(DateTime startDate, DateTime endDate)
        {
            var allTransactionViewModel = new TransactionAccessController(connection);
            var model = new AllTransactionViewModel();
            model.AllTransactions = allTransactionViewModel.GetFilteredTransactionsList(startDate, endDate);
            model.IncomeLists = allTransactionViewModel.GetIncome(startDate, endDate);
            model.ExpenseLists = allTransactionViewModel.GetExpense(startDate, endDate);
            return View(model);
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
        public void AddCsvData()
        {
                using (var StreamReader = new StreamReader(@"C:\Users\santo\Downloads\industry.csv"))
                {
                    using (var csvReader = new CsvReader(StreamReader, CultureInfo.InvariantCulture))
                    {
                        csvReader.Context.RegisterClassMap<CsvTransactionClassMap>();
                        var records = csvReader.GetRecords<CsvTransaction>().ToList();
                        Console.WriteLine(records.Count);
                        foreach(var item in records)
                         {
                            CsvAddTransaction(item.Account, item.Amount, item.Date, item.Note);
                         }
                    }
                }
        }
        public void CsvAddTransaction(string account, int amount, DateTime date, string note)
        {
            Console.WriteLine("CsvAddTransaction Called -----");
                var payl = new MakeExpensePayload { Amount = amount, Date = date, Note = note, AccountName = account };

                var bo = new ExpenseBO(connection);
                bo.MakeExpense(payl);
        }


        public class CsvTransactionClassMap : ClassMap<CsvTransaction>
        {
            public CsvTransactionClassMap()
            {
                Map(m => m.Amount).Name("Amount");
                Map(m => m.Account).Name("Account");
                Map(m => m.Date).Name("Date").TypeConverterOption.Format("dd-mm-yyyy");
                Map(m => m.Note).Name("Note");
            }
        }


        public class CsvTransaction
        {
            public string Account { get; set; }
            public int Amount { get; set; }
            public DateTime Date { get; set; }
            public string Note { get; set; }
        }
    }
}