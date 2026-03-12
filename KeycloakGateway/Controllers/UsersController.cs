using KeycloakGateway.Application.DTOs.Auth;
using KeycloakGateway.Application.DTOs.Users;
using KeycloakGateway.Application.Interfaces;
using KeycloakGateway.Infrastructure.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace KeycloakGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IKeycloakUserService _userService;
        private readonly IExcelImportService _excelImportService;
        private readonly IUserService _userInfoService;
        public UsersController(IKeycloakUserService userService, IExcelImportService excelImportService, IUserService userInfoService   )
        {
            _userService = userService;
            _excelImportService = excelImportService;
            _userInfoService = userInfoService;
        }

        /// <summary>
        /// Tạo user mới trong Keycloak
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            var result = await _userService.CreateUserAsync(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcel(IFormFile file, CancellationToken cancellationToken)
        {
            var result = await _excelImportService.ImportUsersAsync(file, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("đang su dung server: ");
                var accessToken = HttpContext.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                var user = await _userInfoService.GetCurrentUserAsync(accessToken, cancellationToken);

                return Ok(user);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }

        [HttpGet("{userId}/groups")]
        public async Task<IActionResult> GetUserGroups(string userId)
        {
            var groups = await _userInfoService.GetUserGroupsAsync(userId); 

            return Ok(groups);
        }

        /// <summary>
        /// Gán role cho user
        /// </summary>
        //[HttpPost("assign-role")]
        //public async Task<IActionResult> AssignRole(
        //    [FromBody] AssignRoleRequest request,
        //    CancellationToken ct)
        //{
        //    var result = await _userService.AssignRoleAsync(request, ct);

        //    if (!result.IsSuccess)
        //        return BadRequest(result);

        //    return Ok(result);
        //}
    }
}
