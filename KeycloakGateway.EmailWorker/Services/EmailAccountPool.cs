using KeycloakGateway.EmailWorker.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.EmailWorker.Services
{
    public class EmailAccountPool
    {
        private readonly List<EmailAccountState> _accounts;
        private int _index = 0;
        private readonly object _lock = new();

        public EmailAccountPool(IOptions<List<SmtpAccountOptions>> options)
        {
            if (options.Value == null || options.Value.Count == 0)
                throw new InvalidOperationException("No SMTP accounts configured.");

            _accounts = options.Value
                .Select(x => new EmailAccountState
                {
                    Account = x,
                    MaxPerDay = 500
                })
                .ToList();
        }

        public EmailAccountState? GetNextAvailable()
        {
            lock (_lock)
            {
                int attempts = 0;

                while (attempts < _accounts.Count)
                {
                    var account = _accounts[_index];
                    _index = (_index + 1) % _accounts.Count;

                    if (account.IsAvailable())
                        return account;

                    attempts++;
                }

                return null; // Không account nào dùng được
            }
        }

        public void MarkSuccess(EmailAccountState account)
        {
            lock (_lock)
            {
                account.SentToday++;
            }
        }

        public void MarkFailure(EmailAccountState account)
        {
            lock (_lock)
            {
                account.CooldownUntil = DateTime.UtcNow.AddMinutes(10);
            }
        }
    }
}
