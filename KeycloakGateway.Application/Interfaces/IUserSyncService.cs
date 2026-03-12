using KeycloakGateway.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IUserSyncService
    {
        /// <summary>
        /// Đồng bộ user giữa 2 client
        /// </summary>
        Task<Result> SyncUsersBetweenClientsAsync(
            string sourceClientId,
            string targetClientId,
            CancellationToken cancellationToken = default);
    }
}
