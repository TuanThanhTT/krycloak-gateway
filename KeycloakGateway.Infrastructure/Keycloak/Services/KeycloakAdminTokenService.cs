using KeycloakGateway.Application.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Keycloak.Services
{
    public class KeycloakAdminTokenService : Application.Interfaces.IKeycloakAdminTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _options;
        private readonly IMemoryCache _cache;

        public KeycloakAdminTokenService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IOptions<KeycloakOptions> options, IMemoryCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _options = options.Value;
            _cache = cache;
        }

        public string GetAdminAccessToken()
        {
            var authHeader = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["Authorization"]
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authHeader))
                throw new UnauthorizedAccessException("Missing Authorization header");

            if (!authHeader.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Invalid Authorization header");

            return authHeader.Replace("Bearer ", "");
        }

        public async Task<string> GetClientTokenAsync()
        {
          
            const string cacheKey = "keycloak_client_token";

            if (_cache.TryGetValue(cacheKey, out string cachedToken))
                return cachedToken;

            var tokenEndpoint = $"{_options.BaseUrl}/realms/{_options.Realm}/protocol/openid-connect/token";

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _options.ClientId },
                { "client_secret", _options.ClientSecret }
            };

            Console.WriteLine("BaseUrl: " + _options.BaseUrl);
            Console.WriteLine("Realm: " + _options.Realm);
            Console.WriteLine("ClientId: " + _options.ClientId);
            Console.WriteLine("Secret: " + _options.ClientSecret);

            Console.WriteLine("TokenEndpoint: " + tokenEndpoint);

            var content = new FormUrlEncodedContent(parameters);

            var response = await _httpClient.PostAsync(tokenEndpoint, content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var token = tokenResponse!.AccessToken;

            // cache token (trừ hao 30s)
            _cache.Set(cacheKey, token, TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 30));

            return token;
        }
    }
}
