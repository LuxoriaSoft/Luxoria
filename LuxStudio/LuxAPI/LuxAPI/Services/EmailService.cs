using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace LuxAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly string _frontEndUrl;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
            _frontEndUrl = _config["URI:FrontEnd"] ?? throw new Exception("Frontend URL is not set.");
        }
        public string FrontEndUrl => _frontEndUrl;

        public async Task SendMentionEmailAsync(string toEmail, string fromEmail, string message)
        {
            var subject = "Vous avez été mentionné sur Luxoria";
            var htmlBody = $@"
        <h2>Notification de mention</h2>
        <p>Vous avez été mentionné par <strong>{fromEmail}</strong> dans un message :</p>
        <blockquote>{message}</blockquote>
        <p>Connectez-vous pour voir le message complet.</p>
        <p style='font-size:12px; color:#999;'>Si vous ne reconnaissez pas cette notification, ignorez ce message.</p>
    ";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"], "Luxoria"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            using var smtp = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]))
            {
                Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mailMessage);
            _logger.LogInformation("Mention email sent to {Email} from {FromEmail}", toEmail, fromEmail);
        }


        public async Task SendResetPasswordEmailAsync(string toEmail, string resetLink)
{
    var subject = "Réinitialisation de votre mot de passe Luxoria";
    var htmlBody = $@"
    <!DOCTYPE html>
    <html lang='fr'>
    <head>
    <meta charset='UTF-8' />
    <title>Réinitialisation de mot de passe</title>
    </head>
    <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <h2 style='color: #333;'>Bonjour,</h2>
        <p>Vous avez demandé à réinitialiser votre mot de passe pour votre compte Luxoria.</p>
        <p>Cliquez sur le bouton ci-dessous pour définir un nouveau mot de passe. Ce lien sera valide pendant 1 heure.</p>
        <p style='text-align:center; margin: 30px 0;'>
            <a href='{resetLink}' style='background-color: #B91F1E; color: white; padding: 12px 20px; border-radius: 6px; text-decoration: none; font-weight: bold;'>
                Réinitialiser mon mot de passe
            </a>
        </p>
        <p>Si vous n'avez pas demandé cette réinitialisation, vous pouvez ignorer ce message.</p>
        <hr style='margin-top: 30px;' />
        <p style='font-size: 12px; color: #999; text-align: center;'>© Luxoria {DateTime.UtcNow:yyyy} – Ne répondez pas à cet e-mail.</p>
    </div>
    </body>
    </html>";

    var mailMessage = new MailMessage
    {
        From = new MailAddress(_config["Smtp:From"], "Luxoria"),
        Subject = subject,
        Body = htmlBody,
        IsBodyHtml = true
    };

    mailMessage.To.Add(toEmail);

    using var smtp = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]))
    {
        Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
        EnableSsl = true
    };

    await smtp.SendMailAsync(mailMessage);
    _logger.LogInformation("Reset password email sent to {Email}", toEmail);
}


        public async Task SendVerificationCodeAsync(string toEmail, string toName, string code, Guid pendingId)
        {
            var confirmationUrl = $"{_frontEndUrl}/register/confirmation?id={pendingId}&code={code}";

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

        public async Task SendRegistrationInvitationAsync(string toEmail, string collectionName, string registrationUrl, string invitedBy)
        {
            var html = $@"
                <h2>Vous avez été invité à rejoindre une collection sur Luxoria</h2>
                <p><strong>{invitedBy}</strong> vous a invité à accéder à la collection : <strong>{collectionName}</strong>.</p>
                <p>Créez un compte pour y accéder :</p>
                <p style='margin-top:20px;'>
                <a href='{registrationUrl}' style='background:#2d89ef; color:#ffffff; padding:12px 20px; border-radius:6px; text-decoration:none; font-weight:bold;'>
                    Rejoindre Luxoria
                </a>
                </p>
                <p style='font-size:12px; color:#999; margin-top:20px;'>Si vous ne reconnaissez pas cette invitation, ignorez ce message.</p>";
            
            var msg = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"], "Luxoria"),
                Subject = "Invitation à rejoindre Luxoria",
                Body = html,
                IsBodyHtml = true
            };

            msg.To.Add(toEmail);

            using var smtp = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]))
            {
                Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(msg);
            _logger.LogInformation("Invitation à s'enregistrer envoyée à {Email}", toEmail);
        }

        public async Task SendAccessGrantedNotificationAsync(string toEmail, string collectionName, string collectionUrl, string invitedBy)
        {
            var html = $@"
            <h2>Vous avez été ajouté à une collection sur Luxoria</h2>
            <p><strong>{invitedBy}</strong> Vous a donné accès à la collection : <strong>{collectionName}</strong>.</p>
            <p>Accédez-la ici :</p>
            <p style='margin-top:20px;'>
            <a href='{collectionUrl}' style='background:#4caf50; color:#fff; padding:12px 20px; border-radius:6px; text-decoration:none; font-weight:bold;'>
                Voir la collection
            </a>
            </p>
            <p style='font-size:12px; color:#999; margin-top:20px;'>Si vous ne reconnaissez pas cette invitation, ignorez ce message.</p>";

            var msg = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"], "Luxoria"),
                Subject = "Nouvel accès à une collection Luxoria",
                Body = html,
                IsBodyHtml = true
            };

            msg.To.Add(toEmail);

            using var smtp = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]))
            {
                Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(msg);
            _logger.LogInformation("Notification d'accès envoyé à {Email}", toEmail);
        }
    }
}
