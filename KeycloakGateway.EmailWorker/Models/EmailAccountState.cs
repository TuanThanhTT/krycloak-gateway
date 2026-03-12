using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.EmailWorker.Models
{
    public class EmailAccountState
    {
        public SmtpAccountOptions Account { get; set; } = default!;

        public int SentToday { get; set; }

        public int MaxPerDay { get; set; } = 500;

        public DateTime? CooldownUntil { get; set; }

        public bool IsAvailable()
        {
            if (CooldownUntil.HasValue && CooldownUntil > DateTime.UtcNow)
                return false;

            if (SentToday >= MaxPerDay)
                return false;

            return true;
        }
    }
}
