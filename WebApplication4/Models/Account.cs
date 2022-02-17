using Dapper.Contrib.Extensions;

namespace WebApplication4.Models
{
    [Table("account")]
        public class Account
        {
        public int AccountId { get; set; }
        [Key]
        public string AccountName { get; set; }
        public string Type { get; set; }
        public IEnumerable<Account> AccountsList { get; set;}
    }
}
