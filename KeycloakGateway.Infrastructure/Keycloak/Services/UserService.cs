using KeycloakGateway.Application.DTOs.Users;
using KeycloakGateway.Application.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Keycloak.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;    
        private readonly KeycloakOptions _options;  
        private readonly IKeycloakAdminTokenService _tokenService;  

        public UserService(HttpClient httpClient, IOptions<KeycloakOptions> options, IKeycloakAdminTokenService tokenService)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _tokenService = tokenService;
        }   

        public async Task<UserProfileResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Đang sử dụng địa chi keycloak: "+ _options.BaseUrl);
            // 1️⃣ Lấy userId từ JWT
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);

            var userId = jwt.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new Exception("Invalid token");

            // 2️⃣ Lấy admin token
            var adminToken = await _tokenService.GetClientTokenAsync();

            var url =
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Get user failed: {error}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var user = JsonSerializer.Deserialize<KeycloakUser>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new UserProfileResponse
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<List<UserGroupDto>> GetUserGroupsAsync(string userId)
        {
            var accessToken = await _tokenService.GetClientTokenAsync();

            var url = $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}/groups";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<UserGroupDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<UserGroupDto>();
        }
    }
}
