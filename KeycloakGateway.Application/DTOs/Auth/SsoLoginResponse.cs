using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class SsoLoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? RedirectUri { get; set; }
        public string? Message { get; set; }
    }
}
