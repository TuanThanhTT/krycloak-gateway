using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Keycloak.Models
{
    public class KeycloakUserRepresentation
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = default!;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("credentials")]
        public List<KeycloakCredential>? Credentials { get; set; }

        [JsonPropertyName("realmRoles")]
        public List<string>? RealmRoles { get; set; }

        [JsonPropertyName("clientRoles")]
        public Dictionary<string, List<string>>? ClientRoles { get; set; }
    }

    public class KeycloakCredential
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "password";

        [JsonPropertyName("value")]
        public string Value { get; set; } = default!;

        [JsonPropertyName("temporary")]
        public bool Temporary { get; set; } = false;
    }
}
