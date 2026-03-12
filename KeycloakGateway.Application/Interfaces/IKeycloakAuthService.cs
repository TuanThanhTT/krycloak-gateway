using KeycloakGateway.Application.Common;
using KeycloakGateway.Application.DTOs.Auth;
using KeycloakGateway.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IKeycloakAuthService
    {
        /// <summary>
        /// Login user và lấy access token từ Keycloak
        /// </summary>
        Task<Result<TokenResponse>> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default);

        Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    }
}
