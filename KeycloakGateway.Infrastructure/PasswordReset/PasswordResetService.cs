using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using KeycloakGateway.Application.DTOs.Email;
using KeycloakGateway.Application.Interfaces;
using KeycloakGateway.Infrastructure.Keycloak;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace KeycloakGateway.Infrastructure.PasswordReset
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IConfiguration _config;
        private readonly IKeycloakUserService _userService;
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _options;
        private readonly IKeycloakAdminTokenService _tokenService;
        private readonly IEmailProducer _emailProducer;
        private readonly IEmailTemplateService _templateService;
        public PasswordResetService(IConfiguration config, IKeycloakUserService userService,
            HttpClient httpClient, IOptions<KeycloakOptions> options,
            IKeycloakAdminTokenService tokenService, IEmailProducer emailProducer,
            IEmailTemplateService templateService
            )
        {
            _config = config;
            _userService = userService;
            _options = options.Value;
            _httpClient = httpClient;
            _tokenService = tokenService;
            _emailProducer = emailProducer;
            _templateService = templateService;
        }


        public async Task RequestResetAsync(string usernameOrEmail)
        {
            var users = await _userService.SearchUserAsync(usernameOrEmail);

            if (users == null || !users.Any())
                return;

            var user = users.First();

            var token = GenerateResetToken(user.Id);

            var resetUrl = _config["Frontend:ResetPasswordUrl"]
                ?? throw new Exception("Frontend:ResetPasswordUrl not configured");

            var link = $"{resetUrl}?token={Uri.EscapeDataString(token)}";

            Console.WriteLine("Gui den email: "+ user.Email);
            var html = _templateService.RenderTemplate(
                        "ResetPassword",
                        new Dictionary<string, string>
                        {
                            { "username", usernameOrEmail },
                            { "link", link }
                        }
                    );

            await _emailProducer.PublishAsync(new EmailMessage
            {
                To = user.Email,
                Subject = "Reset Password",
                HtmlBody = html
            });

            Console.WriteLine("da gui mail vao queue thanh cong");  
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var userId = ValidateResetToken(token);

            await _userService.ResetPasswordAsync(userId, newPassword);
        }

        private string GenerateResetToken(string userId)
        {
            var claims = new[]
            {
            new Claim("userId", userId),
            new Claim("type", "reset")
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:ResetSecret"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:ResetTokenExpirationMinutes"]!));

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiration,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string ValidateResetToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_config["Jwt:ResetSecret"]!);

            var principal = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out _);

            var type = principal.FindFirst("type")?.Value;

            if (type != "reset")
                throw new SecurityTokenException("Invalid token type");

            return principal.FindFirst("userId")?.Value
                ?? throw new SecurityTokenException("Invalid token");
        }

        public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var userAccessToken = _tokenService.GetAdminAccessToken();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(userAccessToken);

            var userId = jwt.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            if (userId == null)
                throw new Exception("Invalid token");


            var adminToken = await _tokenService.GetClientTokenAsync();

            var url =
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{userId}/reset-password";

            var request = new HttpRequestMessage(HttpMethod.Put, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);

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
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Change password failed: {error}");
            }
        }
    }
}
