using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Services
{
    public class AuthCodeData
    {
        public string AccessToken { get; set; }
        public string CodeChallenge { get; set; }
    }
}
