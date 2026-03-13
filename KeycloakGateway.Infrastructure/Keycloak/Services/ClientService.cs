using KeycloakGateway.Application.DTOs.Auth;
using KeycloakGateway.Application.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Keycloak.Services
{
    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _config;
        private readonly IKeycloakAdminTokenService _tokenService;

        public ClientService(
            IHttpClientFactory httpClientFactory,
            IOptions<KeycloakOptions> options,
            IKeycloakAdminTokenService tokenService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = options.Value;
            _tokenService = tokenService;
        }

        public async Task<string?> GetRedirectUriAsync(string clientId)
        {
            var token = await _tokenService.GetClientTokenAsync();

            var url =
                $"{_config.BaseUrl}/admin/realms/{_config.Realm}/clients?clientId={clientId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);

            var client = doc.RootElement.EnumerateArray().FirstOrDefault();

            if (client.ValueKind == JsonValueKind.Undefined)
                return null;

            var redirectUris = client
                .GetProperty("redirectUris")
                .EnumerateArray()
                .Select(x => x.GetString())
                .ToList();

            return redirectUris.FirstOrDefault();
        }
    }
}
