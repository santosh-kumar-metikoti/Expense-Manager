namespace WebApplication4.Models
{
    public class AllTransactionViewModel
    {
        public List<Transaction> AllTransactions { get; set; }
        public List<Transaction> IncomeLists { get; set; }
        public List<Transaction> ExpenseLists { get; set; }

    }
}
