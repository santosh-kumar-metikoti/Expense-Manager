
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace WebApplication4.CsvHelper
{
    public class CsvHelp
    {
        public void Hello()
        {
            using (var StreamReader = new StreamReader(@"C:\Users\santo\Downloads\industry.csv"))
            {
                using (var csvReader = new CsvReader(StreamReader, CultureInfo.InvariantCulture))
                {
                    csvReader.Context.RegisterClassMap<CsvTransactionClassMap>();
                    var records = csvReader.GetRecords<dynamic>().ToList();
                    Console.WriteLine(records);
                }
            }
        }
    }


    public class CsvTransactionClassMap : ClassMap<CsvTransaction>
    {
        public CsvTransactionClassMap()
        {
            Map(m => m.Amount).Name("Amount");
            Map(m => m.Account).Name("Account");
            Map(m => m.Date).Name("Date");
            Map(m => m.Type).Name("Type");
        }
    }


    public class CsvTransaction 
    { 
        public string Account { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }
}
