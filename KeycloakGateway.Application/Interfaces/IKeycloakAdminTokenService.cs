using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IKeycloakAdminTokenService
    {
        string GetAdminAccessToken();
        Task<string> GetClientTokenAsync();
    }
}
