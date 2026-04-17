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
                var server = _configuration["SmtpSettings:Server"] ?? "smtp.gmail.com";
                var portStr = _configuration["SmtpSettings:Port"] ?? "587";
                int port = int.TryParse(portStr, out int p) ? p : 587;
                
                var email = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];
                var senderName = _configuration["SmtpSettings:SenderName"] ?? "Barrio Inteligente";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException(
                        "Credenciales SMTP no encontradas en la configuración o User Secrets. " +
                        "Verifique que SmtpSettings:SenderEmail y SmtpSettings:Password estén configurados.");
                }

                using var smtpClient = new SmtpClient(server)
                {
                    Port = port,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email, senderName),
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
