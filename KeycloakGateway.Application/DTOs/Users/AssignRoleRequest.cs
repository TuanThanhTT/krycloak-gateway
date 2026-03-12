using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Users
{
    public class AssignRoleRequest
    {
        public string UserId { get; set; } = default!;
        public string RoleName { get; set; } = default!;
    }
}
