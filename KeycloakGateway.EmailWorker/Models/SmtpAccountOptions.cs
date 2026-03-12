

namespace KeycloakGateway.EmailWorker.Models
{
    public class SmtpAccountOptions
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool EnableSsl { get; set; }
        public string From { get; set; } = default!;

    }
}
