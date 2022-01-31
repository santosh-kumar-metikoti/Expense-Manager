namespace WebApplication4.Models
{
    public class AllTransactionViewModel
    {
        public IEnumerable<Transaction> AllTransactions { get; set; }
        public IEnumerable<Transaction> IncomeLists { get; set; }
        public IEnumerable<Transaction> ExpenseLists { get; set; }

    }
}
