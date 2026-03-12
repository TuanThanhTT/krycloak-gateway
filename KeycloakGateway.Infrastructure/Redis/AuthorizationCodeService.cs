using KeycloakGateway.Application.DTOs.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Redis
{
    public class AuthorizationCodeService
    {
        private readonly IDatabase _db;

        public AuthorizationCodeService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SaveCodeAsync(
            string code,
            string accessToken,
            string codeChallenge)
        {
            var data = new AuthCodeData
            {
                AccessToken = accessToken,
                CodeChallenge = codeChallenge
            };

            var json = JsonSerializer.Serialize(data);

            await _db.StringSetAsync(
                $"auth_code:{code}",
                json,
                TimeSpan.FromSeconds(60)); // expiration
        }

        public async Task<AuthCodeData?> GetCodeAsync(string code)
        {
            var value = await _db.StringGetAsync($"auth_code:{code}");

            if (value.IsNullOrEmpty)
                return null;

            await _db.KeyDeleteAsync($"auth_code:{code}");

            return JsonSerializer.Deserialize<AuthCodeData>(value);
        }
    }
}
