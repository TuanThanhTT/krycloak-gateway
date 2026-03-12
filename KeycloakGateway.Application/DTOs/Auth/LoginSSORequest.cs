using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class LoginSSORequest
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string State { get; set; }
        public string CodeChallenge { get; set; }
    }
}
