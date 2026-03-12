using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class AuthCodeStore
    {
        public static Dictionary<string, string> Codes = new();
    }
}

