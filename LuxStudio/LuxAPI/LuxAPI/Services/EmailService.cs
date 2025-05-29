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

        public async Task SendVerificationCodeAsync(string toEmail, string toName, string code, Guid pendingId)
        {
var confirmationUrl = $"http://localhost:5173/register/confirmation?id={pendingId}&code={code}";

var htmlBody = $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Code de vérification</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
  <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
    <h2 style='color: #333;'>Bonjour {toName},</h2>
    <p style='font-size: 16px;'>Merci pour votre inscription sur <strong>Luxoria</strong> !</p>
    <p style='font-size: 16px;'>Voici votre code de vérification :</p>
    <p style='font-size: 32px; font-weight: bold; text-align: center; color: #2d89ef;'>{code}</p>
    <p style='font-size: 14px; color: #666; text-align: center;'>Ce code est valable pendant <strong>10 minutes</strong>.</p>

    <hr style='margin: 30px 0;' />

    <p style='font-size: 16px;'>Cliquez ici pour confirmer automatiquement :</p>
    <p style='margin: 20px 0; text-align: center;'>
      <a href='{confirmationUrl}' style='background-color: #2d89ef; color: #ffffff; padding: 12px 20px; border-radius: 6px; text-decoration: none; font-size: 16px;'>
        ✅ Confirmer mon compte
      </a>
    </p>

    <p style='font-size: 14px; color: #888;'>Si le bouton ne fonctionne pas, copiez-collez ce lien dans votre navigateur :</p>
    <p style='font-size: 12px; color: #555; word-break: break-all;'>{confirmationUrl}</p>

    <hr style='margin: 30px 0;' />
    <p style='font-size: 12px; color: #999; text-align: center;'>© Luxoria {DateTime.UtcNow:yyyy} – Ne répondez pas à cet e-mail.</p>
  </div>
</body>
</html>";



            var smtpClient = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]))
            {
                Credentials = new NetworkCredential(
                    _config["Smtp:Username"],
                    _config["Smtp:Password"]
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"], "Luxoria"),
                Subject = "Votre code de vérification Luxoria",
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("SMTP: Mail de vérification envoyé à {Email}", toEmail);
        }
    }
}
