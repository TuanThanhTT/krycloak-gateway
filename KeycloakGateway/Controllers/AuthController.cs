using KeycloakGateway.Application.DTOs.Auth;
using KeycloakGateway.Application.DTOs.Users;
using KeycloakGateway.Application.Interfaces;
using KeycloakGateway.Infrastructure.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace KeycloakGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Route("")]
    public class AuthController : ControllerBase
    {
        private readonly IKeycloakAuthService _authService;
        private readonly IUserService _userService;
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _config;
        private readonly KeycloakClients _clients;
        private readonly AuthorizationCodeService _authCodeService;
        private readonly IClientService _clientService;
        public AuthController(IKeycloakAuthService authService, IUserService userService, IHttpClientFactory httpClientFactory, IOptions<KeycloakOptions> options, IOptions<KeycloakClients> clients, AuthorizationCodeService authCodeService, IClientService clientService)
        {
            _authService = authService;
            _userService = userService;
            _httpClient = httpClientFactory.CreateClient();
            _config = options.Value;
            _clients = clients.Value;
            _authCodeService = authCodeService;
            _clientService = clientService;
        }

        /// <summary>
        /// Login lấy access token từ Keycloak
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken ct)
        {
            var result = await _authService.LoginAsync(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _authService.RegisterAsync(request);
            return Ok(new { message = "User registered successfully" });
        }


        [HttpPost("login-swagger")]
        public async Task<IActionResult> LoginSwagger([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _authService.LoginAsync(request, ct);

            if (!result.IsSuccess)
                return BadRequest(new SwaggerLoginResponse
                {
                    IsAdmin = false,
                    Message = "Invalid credentials"
                });

            var token = result.Data.AccessToken;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userId = jwt.Claims
                .FirstOrDefault(x => x.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new SwaggerLoginResponse
                {
                    IsAdmin = false,
                    Message = "Invalid token"
                });

            var groups = await _userService.GetUserGroupsAsync(userId);

            var isAdmin = groups.Any(g => g.Name == "ADMIN");

            if (!isAdmin)
            {
                return Ok(new SwaggerLoginResponse
                {
                    IsAdmin = false,
                    Message = "User is not ADMIN"
                });
            }

            return Ok(new SwaggerLoginResponse
            {
                IsAdmin = true,
                AccessToken = token
            });
        }


        /// Endpoint này cho phép các hệ thống tự động đọc cấu hình SSO

        [HttpGet(".well-known/openid-configuration")]
        public IActionResult Discovery()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                issuer = baseUrl,

                authorization_endpoint = $"{baseUrl}/oauth2/authorize",
                token_endpoint = $"{baseUrl}/oauth2/token",
                userinfo_endpoint = $"{baseUrl}/oauth2/userinfo",
                jwks_uri = $"{baseUrl}/oauth2/jwks",
                introspection_endpoint = $"{baseUrl}/oauth2/introspect",
                end_session_endpoint = $"{baseUrl}/oauth2/logout",

                response_types_supported = new[] { "code" },

                grant_types_supported = new[]
                {
                    "authorization_code",
                    "refresh_token"
                },

                scopes_supported = new[]
                {
                    "openid",
                    "profile",
                    "email"
                },

                id_token_signing_alg_values_supported = new[]
                {
                    "RS256"
                }
            });
        }

        // Client sẽ redirect user tới endpoint này.

        //[HttpGet("/oauth2/authorize")]
        //public IActionResult Authorize(
        //    string client_id,
        //    string redirect_uri,
        //    string state,
        //    string scope,
        //    string response_type
        //)
        //{
        //    var loginUrl =
        //        $"http://localhost:3000/login" +
        //        $"?client_id={client_id}" +
        //        $"&redirect_uri={redirect_uri}" +
        //        $"&state={state}" +
        //        $"&scope={scope}";

        //    return Redirect(loginUrl);
        //}

        [HttpGet("/oauth2/authorize")]
        public async Task<IActionResult> Authorize(
            string client_id,
            string state,
            string code_challenge,
            string code_challenge_method)
        {
            //var redirect_uri = await _clientService.GetRedirectUriAsync(client_id);
            //var loginUrl =
            //    _config.SSO_loginUrl +
            //    $"?client_id={client_id}" +
            //    $"&redirect_uri={redirect_uri}" +
            //    $"&state={state}" +
            //    $"&code_challenge={code_challenge}" +
            //    $"&code_challenge_method={code_challenge_method}";

            var redirectUri = await _clientService.GetRedirectUriAsync(client_id);

            if (string.IsNullOrEmpty(redirectUri))
                return BadRequest("invalid_client");

            var query = new Dictionary<string, string?>
            {
                ["client_id"] = client_id,
                ["redirect_uri"] = redirectUri,
                ["state"] = state,
                ["code_challenge"] = code_challenge,
                ["code_challenge_method"] = code_challenge_method
            };

            var loginUrl = QueryHelpers.AddQueryString(
                _config.SSO_loginUrl,
                query);

            return Redirect(loginUrl);
        }

        //Client đổi authorization_code để lấy token.

        //[HttpPost("oauth2/token")]
        //public async Task<IActionResult> Token()
        //{
        //    var form = await Request.ReadFormAsync();

        //    var code = form["code"].ToString();

        //    if (!AuthCodeStore.Codes.ContainsKey(code))
        //        return BadRequest("invalid_code");

        //    var token = AuthCodeStore.Codes[code];

        //    AuthCodeStore.Codes.Remove(code);

        //    return Ok(new
        //    {
        //        access_token = token,
        //        token_type = "Bearer",
        //        expires_in = 3600
        //    });
        //}

        [HttpPost("oauth2/token")]
        public async Task<IActionResult> Token()
        {
            var form = await Request.ReadFormAsync();

            var code = form["code"].ToString();
            var verifier = form["code_verifier"].ToString();

            var data = await _authCodeService.GetCodeAsync(code);

            if (data == null)
                return BadRequest("invalid_code");

            var hashed = Base64UrlEncode(
                SHA256.HashData(
                    Encoding.ASCII.GetBytes(verifier)));

            if (hashed != data.CodeChallenge)
                return BadRequest("invalid_pkce");

            return Ok(new
            {
                access_token = data.AccessToken,
                token_type = "Bearer",
                expires_in = 3600
            });
        }


        //API lấy thông tin user.

        [HttpGet("oauth2/userinfo")]
        public async Task<IActionResult> UserInfo()
        {
            var token = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                _config.UserInfoEndpoint);

            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }


        //API cung cấp public key để verify JWT.

        [HttpGet("oauth2/jwks")]
        public async Task<IActionResult> Jwks()
        {
            var response = await _httpClient.GetAsync(_config.JwksEndpoint);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }



        //API kiểm tra token còn hợp lệ không.

        [HttpPost("oauth2/introspect")]
        public async Task<IActionResult> Introspect()
        {
            var form = await Request.ReadFormAsync();

            var data = new Dictionary<string, string>
            {
                ["token"] = form["token"],
                ["client_id"] = _clients.Auth.ClientId,
                ["client_secret"] = _clients.Auth.ClientSecret
            };

            var response = await _httpClient.PostAsync(
                _config.IntrospectEndpoint,
                new FormUrlEncodedContent(data));

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }


        //logout

        [HttpGet("oauth2/logout")]
        public IActionResult Logout(string? post_logout_redirect_uri)
        {
            var redirectUri = string.IsNullOrEmpty(post_logout_redirect_uri)
                ? "http://localhost:3000"
                : post_logout_redirect_uri;

            var url =
                $"{_config.BaseUrl}/realms/{_config.Realm}/protocol/openid-connect/logout" +
                $"?post_logout_redirect_uri={Uri.EscapeDataString(redirectUri)}";

            return Redirect(url);
        }


        //[HttpPost("loginSSO")]
        //public async Task<IActionResult> Login([FromBody] LoginSSORequest req)
        //{

        //    var clientId = _clients.Auth.ClientId;
        //    var clientSecret = _clients.Auth.ClientSecret;

        //    var form = new Dictionary<string, string>
        //    {
        //        {"grant_type","password"},
        //        {"client_id", clientId},
        //        {"client_secret", clientSecret},
        //        {"username", req.Username},
        //        {"password", req.Password}
        //    };

        //    var response = await _httpClient.PostAsync(
        //        _config.TokenEndpoint,
        //        new FormUrlEncodedContent(form));

        //    var content = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //        return Unauthorized();

        //    var token = System.Text.Json.JsonDocument
        //        .Parse(content)
        //        .RootElement
        //        .GetProperty("access_token")
        //        .GetString();

        //    // tạo authorization code
        //    var code = Guid.NewGuid().ToString();

        //    // lưu code -> token
        //    AuthCodeStore.Codes[code] = token;

        //    var redirect = $"{req.RedirectUri}?code={code}&state={req.State}";

        //    return Ok(new
        //    {
        //        success = true,
        //        redirect = redirect
        //    });
        //}


        [HttpPost("loginSSO")]
        public async Task<IActionResult> Login([FromBody] LoginSSORequest req)
        {
            var form = new Dictionary<string, string>
            {
                {"grant_type","password"},
                {"client_id", _clients.Auth.ClientId},
                {"client_secret", _clients.Auth.ClientSecret},
                {"username", req.Username},
                {"password", req.Password}
            };

            var response = await _httpClient.PostAsync(
                _config.TokenEndpoint,
                new FormUrlEncodedContent(form));

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return Unauthorized();

            var json = JsonDocument.Parse(content);

            var accessToken = json.RootElement
                .GetProperty("access_token")
                .GetString();

            var code = Guid.NewGuid().ToString();

            await _authCodeService.SaveCodeAsync(
                code,
                accessToken,
                req.CodeChallenge);

            var redirect =
                $"{req.RedirectUri}?code={code}&state={req.State}";

            return Ok(new
            {
                success = true,
                redirect
            });
        }

        public static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
