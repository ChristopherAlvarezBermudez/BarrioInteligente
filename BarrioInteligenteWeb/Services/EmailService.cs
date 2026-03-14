using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BarrioInteligenteWeb.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            try
            {
                var emailFrom = _configuration["EmailSettings:From"];
                var password = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(emailFrom) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("EmailSettings not configured correctly in appsettings.json.");
                    return;
                }

                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailFrom, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom, "Barrio Inteligente"),
                    Subject = asunto,
                    Body = cuerpoHtml,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(new MailAddress(destinatario));

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email enviado exitosamente a {Destinatario}", destinatario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {Destinatario}", destinatario);
                throw; // Rethrow to let the controller handle it gracefully if needed
            }
        }
    }
}
