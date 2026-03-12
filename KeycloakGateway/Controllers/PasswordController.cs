using KeycloakGateway.Application.DTOs.Auth;
using KeycloakGateway.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakGateway.Controllers
{
    [ApiController]
    [Route("api/password")]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;

        public PasswordController(IPasswordResetService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }

        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestReset([FromBody] string usernameOrEmail)
        {
            await _passwordResetService.RequestResetAsync(usernameOrEmail);
            return Ok();
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _passwordResetService.ResetPasswordAsync(
                request.Token,
                request.NewPassword);

            return Ok();
        }

        [Authorize]
        [HttpPost("change")]
        public async Task<IActionResult> ChangePassword(
          [FromBody] ChangePasswordRequest request,
          CancellationToken cancellationToken)
        {
          
            await _passwordResetService.ChangePasswordAsync(request.CurrentPassword, request.NewPassword);

            return Ok(new { message = "Password changed successfully." });
        }

    }
}
