using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Kheti.KhetiUtils
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                string host = _configuration["Smtp:Host"];
                int port = int.Parse(_configuration["Smtp:Port"]);
                string username = _configuration["Smtp:Username"];
                string password = _configuration["Smtp:Password"];

                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(username);
                    message.To.Add(email);
                    message.Subject = subject;
                    message.Body = htmlMessage;
                    message.IsBodyHtml = true;

                    using (SmtpClient smtpClient = new SmtpClient(host, port))
                    {
                        smtpClient.Credentials = new NetworkCredential(username, password);
                        smtpClient.EnableSsl = true;

                        smtpClient.Send(message);
                    }
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}