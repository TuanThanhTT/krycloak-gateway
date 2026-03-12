using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Keycloak
{
    public class KeycloakOptions
    {
        public const string SectionName = "Keycloak";

        /// <summary>
        /// Base URL của Keycloak
        /// ví dụ: http://localhost:8080
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Realm làm việc
        /// </summary>
        public string Realm { get; set; } = string.Empty;

        /// <summary>
        /// ClientId dùng để login user
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) Client secret nếu sau này cần
        /// hiện tại bạn KHÔNG dùng
        /// </summary>
        public string? ClientSecret { get; set; }

        public string DefaultGroupUserRegister { get; set; } = string.Empty;    
    }
}
