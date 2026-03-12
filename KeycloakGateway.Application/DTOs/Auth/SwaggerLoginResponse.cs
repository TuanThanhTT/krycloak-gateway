using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class SwaggerLoginResponse
    {
        public bool IsAdmin { get; set; }

        public string? AccessToken { get; set; }

        public string? Message { get; set; }
    }
}
