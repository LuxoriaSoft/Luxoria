using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace LuxAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendVerificationCodeAsync(string toEmail, string toName, string code)
        {
            var smtpClient = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]))
            {
                Credentials = new NetworkCredential(
                    _config["Smtp:Username"],
                    _config["Smtp:Password"]
                ),
                EnableSsl = true
            };

            var htmlBody = $@"
        <!DOCTYPE html>
        <html lang='fr'>
        <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Code de vérification</title>
        </head>
        <body style='font-family:Arial,sans-serif; background:#f5f5f5; padding:20px;'>
        <div style='max-width:600px; margin:auto; background:#ffffff; padding:30px; border-radius:8px; box-shadow:0 2px 4px rgba(0,0,0,0.1);'>
            <h2 style='color:#333;'>Bonjour {toName},</h2>
            <p style='font-size:16px;'>Voici votre code de vérification :</p>
            <p style='font-size:32px; font-weight:bold; text-align:center; color:#2d89ef;'>{code}</p>
            <p style='font-size:14px; color:#666;'>Ce code est valable pendant <strong>10 minutes</strong>.</p>
            <hr style='margin:30px 0;' />
            <p style='font-size:12px; color:#999; text-align:center;'>© Luxoria {DateTime.UtcNow:yyyy} – Ne répondez pas à cet e-mail.</p>
        </div>
        </body>
        </html>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"], "Luxoria"),
                Subject = "Votre code de vérification Luxoria",
                Body = htmlBody,
                IsBodyHtml = true // ✅ important pour activer le HTML
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("SMTP: Code HTML envoyé à {Email}", toEmail);
        }

    }
}
