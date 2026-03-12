using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Auth
{
    public class KeycloakOptions
    {
        public string BaseUrl { get; set; }
        public string Realm { get; set; }

        public string AuthEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string JwksEndpoint { get; set; }

        public string ClientSecret { get; set; }
        public string IntrospectEndpoint { get; set; } = default!;
    }
}
