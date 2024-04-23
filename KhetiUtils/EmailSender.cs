using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Kheti.KhetiUtils
{
    // Class for sending emails
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        // Constructor to initialize configuration
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Method to send email asynchronously
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // Retrieve SMTP server configuration from app settings
                string host = _configuration["Smtp:Host"];
                int port = int.Parse(_configuration["Smtp:Port"]);
                string username = _configuration["Smtp:Username"];
                string password = _configuration["Smtp:Password"];

                // Create a new MailMessage instance
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(username);  // Set email sender
                    message.To.Add(email);   // Set email recipient
                    message.Subject = subject;  // Set email subject
                    message.Body = htmlMessage;  // Set email body as HTML
                    message.IsBodyHtml = true;

                    using (SmtpClient smtpClient = new SmtpClient(host, port))  // Create a new SmtpClient instance
                    {
                        smtpClient.Credentials = new NetworkCredential(username, password); // Set SMTP credentials
                        smtpClient.EnableSsl = true;  // Enable SSL encryption

                        smtpClient.Send(message);  // Send the email message
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