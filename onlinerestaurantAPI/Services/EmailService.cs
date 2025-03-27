using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OnlineRestaurantAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPortString = _configuration["EmailSettings:SmtpPort"];
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];

            if (string.IsNullOrEmpty(smtpPortString) || !int.TryParse(smtpPortString, out int smtpPort))
            {
                throw new InvalidOperationException("SMTP port is missing or invalid.");
            }

            if (string.IsNullOrEmpty(smtpServer) ||
                string.IsNullOrEmpty(smtpUsername) ||
                string.IsNullOrEmpty(smtpPassword) ||
                string.IsNullOrEmpty(senderEmail))
            {
                throw new InvalidOperationException("One or more required email configuration values are missing.");
            }

            using (var client = new SmtpClient(smtpServer))
            {
                client.Port = smtpPort;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to send email: {ex.Message}");
                }
            }
        }
    }
}
