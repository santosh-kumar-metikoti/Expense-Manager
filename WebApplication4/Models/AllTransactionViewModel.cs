using Dapper.Contrib.Extensions;

namespace WebApplication4.Models
{
    [Table("transaction")]
    public class AllTransactionViewModel
    {
        public IEnumerable<AllTransactionViewModel> AllTransactions { get; set; }
        public IEnumerable<AllTransactionViewModel> IncomeLists { get; set; }
        public IEnumerable<AllTransactionViewModel> ExpenseLists { get; set; }

        public int Transaction_Id { get; set; }
        public string Account_Name { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public double Income { get; set; }
        public double Expense { get; set; }

    }
}
