 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.DTOs.Users
{
    public class CreateUserRequest
    {
      
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Role cần gán sau khi tạo
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Nếu là client role
        /// </summary>
        public string? ClientId { get; set; }
    }
}
