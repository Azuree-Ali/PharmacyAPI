using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace PharmacyAPI.Utils
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage)
        {
            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Email"],
                    _configuration["EmailSettings:Password"])
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(
                    _configuration["EmailSettings:Email"]!),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(email);

            await client.SendMailAsync(mail);
        }
    }
}