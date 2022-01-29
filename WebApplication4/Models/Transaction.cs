using Npgsql;

namespace WebApplication4.Models
{
    public class Transaction
    {
        public int Transaction_Id { get; set; }
        public string Account_Name { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public int Income { get; set; }
        public int Expense { get; set; }

    }
}
