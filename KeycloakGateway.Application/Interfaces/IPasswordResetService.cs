using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IPasswordResetService
    {
        Task RequestResetAsync(string usernameOrEmail);
        Task ResetPasswordAsync(string token, string newPassword);

        Task ChangePasswordAsync(
           string currentPassword,
           string newPassword,
           CancellationToken cancellationToken = default);
    }
}
