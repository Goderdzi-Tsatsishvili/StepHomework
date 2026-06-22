using MimeKit;
using MimeKit.Text;
using System.Net.Mail;
using WebSaklaso.Exceptions;
using WebSaklaso.Models.Notification;
using WebSaklaso.Service.Contracts;

namespace WebSaklaso.Service
{
    public class EmailService(IConfiguration config, ISmtpClientService smtpClient, ILogger<EmailService> logger) : IEmailService
    {
        public async Task<SendEmailResponseDto> Send(string to, string subject, string body)
        {
            try
            {
                logger.LogInformation("Starting to send email to {Revipient}", to);

                ValidateAddressWhereEmailSent(to);
                logger.LogInformation("Validated Recipient Email: {Recipient}", to);

                var normalizeSubject = NormalizeSubject(subject);
                logger.LogInformation("Normalized Subject: {Subject}", subject);

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(config["EmailSettings:Sender"]));
                email.To.Add(MailboxAddress.Parse(to.Trim()));
                email.Subject = normalizeSubject;
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                logger.LogInformation("Connection to SMTP Server: {Server}:{Port}", config["EmailSettings:SmtpServer"], config["EmailSettings:Port"]);

                await smtpClient.ConnectAsync(
                    config["EmailSettings:SmtpServer"], 
                    int.Parse(config["EmailSettings:Port"]), 
                    bool.Parse(config["EmailSettings:UseSsl"]));

                logger.LogInformation("Authenticating with SMTP server...");

                await smtpClient.AuthenticateAsync(
                    config["EmailSettings:Username"],
                    config["EmailSettings:Password"]
                );

                logger.LogInformation("Sending email to {Recipient}", to);
                await smtpClient.SendAsync(email);

                logger.LogInformation("Disconnecting from SMTP Server...");
                await smtpClient.DisconnectAsync(true);

                logger.LogInformation("Email sent successfully to {Recipient}", to);

                return new SendEmailResponseDto(true, $"Message successfully sent to: {to}");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {Recipient}: {Message}", to, ex.Message);
                return new SendEmailResponseDto(success: false, message: ex.Message, error: ex);
            }
        }

        private string NormalizeSubject(string subject)
        {
            return string.IsNullOrWhiteSpace(subject) ? string.Empty : subject.Trim();
        }

        private void ValidateAddressWhereEmailSent(string to)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new BadRequestException("Email address cannot be empty.");

            try
            {
                var mailAddress = new MailAddress(to);
                if (!mailAddress.Address.Contains("@") || !mailAddress.Address.Contains("."))
                    throw new BadRequestException($"Invalid email address format {to}.");
            }
            catch
            {
                throw new BadRequestException($"Sending email {to} must be a valid email address.");
            }
        }
    }
}
