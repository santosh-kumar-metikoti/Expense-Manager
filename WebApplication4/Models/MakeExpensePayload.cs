namespace WebApplication4.Models
{
    public class MakeExpensePayload
    {
            public string AccountName { get; set; }
            public int Amount { get; set; }
            public string Note { get; set; }
            public DateTime Date { get; set; }
            public int Account_Id { get; set; }
    }
}
