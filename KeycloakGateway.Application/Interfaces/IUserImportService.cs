using KeycloakGateway.Application.Common;
using KeycloakGateway.Application.DTOs.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IUserImportService
    {
        /// <summary>
        /// Import danh sách user từ Excel
        /// </summary>
        Task<Result> ImportUsersAsync(
            IEnumerable<ImportUserDto> users,
            CancellationToken cancellationToken = default);
    }
}
