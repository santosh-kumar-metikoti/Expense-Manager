using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication4.Models
{
    [Table("transaction")]
    public class Transaction
    {
        [Key]
        public int Transaction_Id { get; set; }
        public int Account_Id { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }

    }
}
