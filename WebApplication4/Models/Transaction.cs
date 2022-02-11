using Npgsql;

namespace WebApplication4.Models
{
    public class Transaction
    {
        public int Transaction_Id { get; set; }
        public int Account_Id { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }

    }
}
