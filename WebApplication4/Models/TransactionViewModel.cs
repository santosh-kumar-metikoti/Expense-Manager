namespace WebApplication4.Models
{
    public class TransactionViewModel
    {
        public int Transaction_Id { get; set; }
        public string Account_Name { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public int Income { get; set; }
        public int Expense { get; set; }
        public List<Account> AccountList { get; set; }
        public IEnumerable<TransactionViewModel> Transactions { get; set; }

    }
}