namespace WebApplication4.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string AccountName { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public int Income { get; set; }
        public int Expense { get; set; }
    }
}
