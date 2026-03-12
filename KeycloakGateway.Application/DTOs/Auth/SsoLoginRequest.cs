using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class SsoLoginRequest
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public string RedirectUri { get; set; } = default!;
    }
}
