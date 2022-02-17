﻿
using Dapper.Contrib.Extensions;

namespace WebApplication4.Models
{

    [Table("transaction")]
    public class TransactionViewModel
    {
        [Key]
        public int Transaction_Id { get; set; }
        public string Account_Name { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public IEnumerable<TransactionViewModel> Transactions { get; set; }

    }
}