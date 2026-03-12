using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Users
{
    public class UserGroupDto
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Path { get; set; } = default!;
    }
}
