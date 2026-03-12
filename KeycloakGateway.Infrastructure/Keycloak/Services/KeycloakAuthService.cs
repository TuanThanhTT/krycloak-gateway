using KeycloakGateway.Application.Common;
using KeycloakGateway.Application.DTOs.Auth;
using KeycloakGateway.Application.DTOs.Users;
using KeycloakGateway.Application.Interfaces;
using KeycloakGateway.Infrastructure.Keycloak.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Keycloak.Services
{
    public class KeycloakAuthService : IKeycloakAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _options;
        private readonly IKeycloakAdminTokenService _adminTokenService; 
        public KeycloakAuthService(
            HttpClient httpClient,
            IOptions<KeycloakOptions> options, IKeycloakAdminTokenService keycloakAdminTokenService)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _adminTokenService = keycloakAdminTokenService;
        }
        public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var url =
            $"{_options.BaseUrl}/realms/{_options.Realm}/protocol/openid-connect/token";

            var form = new Dictionary<string, string>
            {
                ["client_id"] = request.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["username"] = request.Username,
                ["password"] = request.Password,
                ["grant_type"] = "password"
            };

            using var content = new FormUrlEncodedContent(form);

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            //if (!response.IsSuccessStatusCode)
            //    throw new Exception($"Keycloak login failed: {json}");

            //if (!response.IsSuccessStatusCode)
            //{
            //    // Parse lỗi từ Keycloak
            //    var error = JsonSerializer.Deserialize<KeycloakErrorResponse>(json,
            //        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //    if (error?.Error == "invalid_grant")
            //    {
            //        return Result<TokenResponse>.Failure("Sai tên đăng nhập hoặc mật khẩu.");
            //    }

            //    return Result<TokenResponse>.Failure("Không thể đăng nhập. Vui lòng thử lại.");
            //}

            if (!response.IsSuccessStatusCode)
            {
                return Result<TokenResponse>.Failure(json);
            }


            var result = JsonSerializer.Deserialize<TokenResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Result<TokenResponse>.Success(result);
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var adminToken = await _adminTokenService.GetClientTokenAsync();

            var url = $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users";

            var createUserRequest = new HttpRequestMessage(HttpMethod.Post, url);
            createUserRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);

            var body = new
            {
                username = request.Username,
                email = request.Email,
                firstName = request.FirstName,
                lastName = request.LastName,
                enabled = true,
                emailVerified = true
            };

            createUserRequest.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(createUserRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Create user failed: {error}");
            }

            // 🔥 Lấy userId từ Location header
            var location = response.Headers.Location?.ToString();
            var userId = location?.Split('/').Last();

            if (string.IsNullOrEmpty(userId))
                throw new Exception("Cannot retrieve created user id");

            // 2️⃣ Set password
            await SetPasswordAsync(userId, request.Password, adminToken);
            //Them vao Group danh cho user đăng ký
            await AddUserToGroupAsync(userId, _options.DefaultGroupUserRegister, adminToken);
        }
        private async Task AddUserToGroupAsync(string userId, string groupId, string adminToken)
        {
            var url =
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}/groups/{groupId}";

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Add user to group failed: {error}");
            }
        }
        private async Task SetPasswordAsync(string userId, string password, string adminToken)
        {
            var url =
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}/reset-password";

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);

            request.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Set password failed: {error}");
            }
        }
    }

    public class KeycloakErrorResponse
    {
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
}
