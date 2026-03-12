using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Import
{
    public class ImportUserDto
    {
        public string Username { get; set; } = default!;
        public bool Enabled { get; set; } = true;
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public List<UserCredentialDto> Credentials { get; set; } = new();
    }

    public class UserCredentialDto
    {
        public string Type { get; set; } = "password";
        public string Value { get; set; } = default!;
        public bool Temporary { get; set; } = false;
    }
}
