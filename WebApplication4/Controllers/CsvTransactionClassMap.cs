
using CsvHelper.Configuration;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class CsvTransactionClassMap : ClassMap<CsvTransaction>
    {
        public CsvTransactionClassMap()
        {
            Map(m => m.Amount).Name("Amount");
            Map(m => m.Account).Name("Account");
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("dd-mm-yyyy");
            Map(m => m.Note).Name("Note");
        }
    }
}
