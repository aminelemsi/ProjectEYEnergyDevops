using System.Net.Mail;
using System.Net;


namespace EY.Energy.Application.EmailConfiguration
{
    public class EmailService : IEmailService
    {

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("ouni.mohaaamed@gmail.com", "dref mdfj bepc hyne");

                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress("ouni.mohaaamed@gmail.com", "EY Energy");
                        mailMessage.To.Add(email);
                        mailMessage.Subject = subject;
                        mailMessage.Body = message;
                        mailMessage.IsBodyHtml=true;


                        await client.SendMailAsync(mailMessage);
                    }
                }
            }
            catch (SmtpException ex)
            {

                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw;
            }
        }
    }

}
