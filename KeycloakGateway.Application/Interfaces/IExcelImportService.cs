using KeycloakGateway.Application.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IExcelImportService
    {
        Task<Result<int>> ImportUsersAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
} 
