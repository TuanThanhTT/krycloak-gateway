using KeycloakGateway.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default);
        Task<List<UserGroupDto>> GetUserGroupsAsync(string userId);
    }
}
