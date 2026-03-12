using ClosedXML.Excel;
using KeycloakGateway.Application.Common;
using KeycloakGateway.Application.DTOs.Users;
using KeycloakGateway.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Infrastructure.Excel
{
    public class ExcelImportService : IExcelImportService
    {
        private readonly IKeycloakUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExcelImportService(
            IKeycloakUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<int>> ImportUsersAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Result<int>.Failure("File is empty");

                int successCount = 0;

                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // skip header

                foreach (var row in rows)
                {
                    var username = row.Cell(1).GetString().Trim();
                    var email = row.Cell(2).GetString().Trim();
                    var firstName = row.Cell(3).GetString().Trim();
                    var lastName = row.Cell(4).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(username))
                        continue;

                    var request = new CreateUserRequest
                    {
                        Username = username,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Password = username // 🔥 password mặc định
                    };

                    var result = await _userService.CreateUserAsync(
                        request,
                        cancellationToken);

                    if (result.IsSuccess)
                        successCount++;
                }

                return Result<int>.Success(successCount);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }
    }
}
