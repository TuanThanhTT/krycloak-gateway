using KeycloakGateway.Application.Common;
using KeycloakGateway.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IKeycloakUserService
    {
        /// <summary>
        /// Tạo user mới trên Keycloak
        /// </summary>
        Task<Result<string>> CreateUserAsync(
            CreateUserRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gán role cho user
        /// </summary>
        Task<Result> AssignRoleAsync(
            AssignRoleRequest request,
            CancellationToken cancellationToken = default);


        /// <summary>
        /// Tìm user theo username hoặc email
        /// </summary>
        Task<Result<KeycloakUserDto?>> FindByUsernameOrEmailAsync(
            string usernameOrEmail,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reset mật khẩu user
        /// </summary>
        Task<Result> ResetPasswordAsync(
            string userId,
            string newPassword,
            CancellationToken cancellationToken = default);

        Task<List<KeycloakUserDto>> SearchUserAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    }
}
