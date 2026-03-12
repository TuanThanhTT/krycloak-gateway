using KeycloakGateway.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly IUserSyncService _syncService;

        public SyncController(IUserSyncService syncService)
        {
            _syncService = syncService;
        }

        /// <summary>
        /// Đồng bộ user giữa các client
        /// </summary>
        //[HttpPost("clients")]
        //public async Task<IActionResult> SyncClients(CancellationToken ct)
        //{
        //    var result = await _syncService.SyncUsersAsync(ct);

        //    if (!result.IsSuccess)
        //        return BadRequest(result);

        //    return Ok(result);
        //}
    }
}
