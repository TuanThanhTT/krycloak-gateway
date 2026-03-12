using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = default!;

        public string Password { get; set; } = default!;

        public string ClientId { get; set; } = default!;

       
    }
}
