using Npgsql;
using Quartz;
using WebApplication4.Models;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using Mjml.AspNetCore;

namespace WebApplication4.Schedulers
{
    public class RemindersJob : IJob
    {
        NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Username=postgres;Password=1234;Database=ExpenseManagerDB");
        public readonly IMjmlServices _mjmlServices;
        public Task Execute(IJobExecutionContext context)
        {
            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate.AddDays(-7);

            var model = new AllTransactionViewModel();
                model = emailAllTransactionsList(startDate, endDate);
                Console.WriteLine(model.AllTransactions);

                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("santosh kumar", "santosh.metikoti@gmail.com"));
                message.To.Add(MailboxAddress.Parse("santoshnani121@gmail.com"));
                message.Subject = "Transactions between " + startDate + "and " + endDate;

                var bodyBuilder = new BodyBuilder();
            
            string str = $"<mjml>< mj - body >< mj - section >< mj - column >< mj - table >< tr >< th > Account </ th >< th > Amount </ th >< th > Date </ th >< th > Type </ th > </ tr > <tr> ";
            foreach (var item in model.AllTransactions)
            {
                str += $" < tr >< td >{item.Account_Name}</ td >< td  > {item.Amount}</ td >< td >{item.Date}</ td ><td>{item.Type}</td></ tr >";
            }
            str += $"</tr></ mj - table ></ mj - column ></ mj - section ></ mj - body ></ mjml >";
           str += $"This is some plain text";
            var result = _mjmlServices.Render(str);
            bodyBuilder.HtmlBody += result;

            message.Body = bodyBuilder.ToMessageBody();
            SmtpClient client = new SmtpClient();
                try
                {
                    client.Connect("Smtp.gmail.com", 465, true);
                    client.Authenticate("santosh.metikoti@gmail.com", "12345");
                    client.Send(message);
                    Console.WriteLine("----- EMAIL SENT!! -----");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();

                }

            return Task.CompletedTask;

        }
        public AllTransactionViewModel emailAllTransactionsList(DateTime startDate, DateTime endDate)
        {
            var allTransactionViewModel = new Controllers.TransactionAccessController(connection);
            var model = new AllTransactionViewModel();
            model.AllTransactions = allTransactionViewModel.GetFilteredTransactionsList(startDate, endDate);
            model.IncomeLists = allTransactionViewModel.GetIncome(startDate, endDate);
            model.ExpenseLists = allTransactionViewModel.GetExpense(startDate, endDate);
            return model;
        }

    }
}
