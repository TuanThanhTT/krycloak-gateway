using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class TokenRequest
    {
        public string GrantType { get; set; }
        public string Code { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
    }
}
