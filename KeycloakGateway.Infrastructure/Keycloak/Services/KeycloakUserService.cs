using KeycloakGateway.Application.Common;
using KeycloakGateway.Application.DTOs.Users;
using KeycloakGateway.Application.Interfaces;
using KeycloakGateway.Infrastructure.Keycloak.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace KeycloakGateway.Infrastructure.Keycloak.Services
{
    public class KeycloakUserService : Application.Interfaces.IKeycloakUserService
    {
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _options;
        private readonly IKeycloakAdminTokenService _tokenService;
        private readonly IMemoryCache _cache;

        public KeycloakUserService(
            HttpClient httpClient,
            IOptions<KeycloakOptions> options,
            IKeycloakAdminTokenService tokenService, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _tokenService = tokenService;
            _cache = cache;
        }

        public async Task<Result> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<string>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = _tokenService.GetAdminAccessToken();

                var url =
                    $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users";

                var payload = new
                {
                    username = request.Username,
                    email = request.Email,
                    enabled = true,
                    emailVerified = true,
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    requiredActions = new string[] { },
                    credentials = new[]
                    {
                new
                {
                    type = "password",
                    value = request.Password,
                    temporary = false
                }
            }
                };

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                httpRequest.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    return Result<string>.Failure($"Create user failed: {error}");
                }

                var location = response.Headers.Location?.ToString();
                var userId = location?.Split('/').Last();

                return Result<string>.Success(userId ?? string.Empty);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message);
            }
        }

        public async Task<Result<KeycloakUserDto?>> FindByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        {
            var accessToken = await _tokenService.GetClientTokenAsync();

            var url =
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users" +
                $"?search={usernameOrEmail}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Result<KeycloakUserDto?>.Failure("Keycloak error");

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var users = JsonSerializer.Deserialize<List<KeycloakUserRepresentation>>(json);

            var user = users?.FirstOrDefault();

            if (user == null)
                return Result<KeycloakUserDto?>.Success(null);

            return Result<KeycloakUserDto?>.Success(new KeycloakUserDto
            {
                Id = user.Id!,
                Username = user.Username!,
                Email = user.Email!
            });
        }

        public async Task<Result> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
        {
            var accessToken = await _tokenService.GetClientTokenAsync();

            var url =
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}/reset-password";

            var request = new HttpRequestMessage(HttpMethod.Put, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var body = new
            {
                type = "password",
                value = newPassword,
                temporary = false
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Result.Failure("Failed to reset password");

            return Result.Success();
        }


        public async Task<List<KeycloakUserDto>> SearchUserAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        {
            //var accessToken = await _tokenService.GetClientTokenAsync();

            //var url =
            //    $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users?search={usernameOrEmail}";

            //var request = new HttpRequestMessage(HttpMethod.Get, url);

            //request.Headers.Authorization =
            //    new AuthenticationHeaderValue("Bearer", accessToken);

            //var response = await _httpClient.SendAsync(request, cancellationToken);


            //if (!response.IsSuccessStatusCode)
            //    return new List<KeycloakUserDto>();

            //var json = await response.Content.ReadAsStringAsync(cancellationToken);

            //var users = JsonSerializer.Deserialize<List<KeycloakUserRepresentation>>(json);
            //Console.WriteLine("So luong ket qua tim kiem thay user la: "+ users.Count());
            //Console.WriteLine("Email cua tai khoan resset la: " + users.First().Email);

            //return users?
            //    .Select(u => new KeycloakUserDto
            //    {
            //        Id = u.Id!,
            //        Username = u.Username!,
            //        Email = u.Email!
            //    })
            //    .ToList()
            //    ?? new List<KeycloakUserDto>();


            var accessToken = await _tokenService.GetClientTokenAsync();

            var url =
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users" +
                $"?username={usernameOrEmail}&exact=true";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new List<KeycloakUserDto>();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var users = JsonSerializer.Deserialize<List<KeycloakUserRepresentation>>(json);

            if (users == null || users.Count == 0)
                return new List<KeycloakUserDto>();

            return users
                .Select(u => new KeycloakUserDto
                {
                    Id = u.Id!,
                    Username = u.Username!,
                    Email = u.Email!
                })
                .ToList();
        }

    }
}
