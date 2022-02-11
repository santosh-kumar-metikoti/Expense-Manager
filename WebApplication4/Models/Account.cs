namespace WebApplication4.Models
{
        public class Account
        {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string Type { get; set; }
        public object Account_Name { get; internal set; }
        public IEnumerable<Account> AccountsList { get; set;}
    }
}
