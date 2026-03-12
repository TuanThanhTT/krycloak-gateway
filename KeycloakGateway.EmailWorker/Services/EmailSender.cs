using KeycloakGateway.Application.DTOs.Email;
using KeycloakGateway.EmailWorker.Models;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace KeycloakGateway.EmailWorker.Services
{
    public class EmailSender
    {
        private readonly EmailAccountPool _accountPool;
        private readonly ILogger<EmailSender> _logger;
        private readonly EmailBrandingOptions _branding;

        public EmailSender(
            EmailAccountPool accountPool,
            ILogger<EmailSender> logger,
            IOptions<EmailBrandingOptions> brandingOptions  
            )
        {
            _accountPool = accountPool;
            _logger = logger;
            _branding = brandingOptions.Value;  
        }

        public async Task<bool> SendAsync(EmailMessage message)
        {
            if (message == null ||
                string.IsNullOrWhiteSpace(message.To) ||
                string.IsNullOrWhiteSpace(message.Subject))
            {
                _logger.LogWarning("Invalid email message.");
                return false;
            }
            Console.WriteLine("đang gửi email đến địa chỉ: "+ message.To);
            return await SendAsync(
                message.To,
                message.Subject,
                message.HtmlBody);
        }

        public async Task<bool> SendAsync(
            string to,
            string subject,
            string htmlBody)
        {
            const int maxAttempts = 5;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                var accountState = _accountPool.GetNextAvailable();

                if (accountState == null)
                {
                    _logger.LogError("No available SMTP accounts.");
                    return false;
                }

                try
                {
                    await SendMailInternal(
                        accountState.Account,
                        to,
                        subject,
                        htmlBody);

                    _accountPool.MarkSuccess(accountState);

                    _logger.LogInformation(
                        $"Email sent using {accountState.Account.Username}");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SMTP sending failed.");

                    _accountPool.MarkFailure(accountState);
                    attempt++;
                }
            }

            _logger.LogError("All retry attempts failed.");
            return false;
        }

        private async Task SendMailInternal(SmtpAccountOptions account, string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();

            var displayName = $"{_branding.DisplayName} - {_branding.Organization}";
            Console.WriteLine("DisplayName hiện tại à: "+ displayName);

            message.From.Add(new MailboxAddress(displayName, account.From));

            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlBody ?? string.Empty
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            // 🔥 CHỌN ĐÚNG SSL MODE
            var socketOption = account.Port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(account.Host, account.Port, socketOption);

            // 🔥 CHỈ AUTH NẾU SERVER SUPPORT
            if (client.Capabilities.HasFlag(MailKit.Net.Smtp.SmtpCapabilities.Authentication))
            {
                await client.AuthenticateAsync(account.Username, account.Password);
            }
            else
            {
                _logger.LogWarning("SMTP server does not support authentication.");
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
