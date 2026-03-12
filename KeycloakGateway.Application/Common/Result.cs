using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Common
{
    
    /// <summary>
    /// Result không trả dữ liệu
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; init; }
        public string? Error { get; init; }

        public static Result Success()
            => new() { IsSuccess = true };

        public static Result Failure(string error)
            => new() { IsSuccess = false, Error = error };
    }

    /// <summary>
    /// Result có trả dữ liệu
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; init; }
        public string? Error { get; init; }
        public T? Data { get; init; }

        public static Result<T> Success(T data)
            => new()
            {
                IsSuccess = true,
                Data = data
            };

        public static Result<T> Failure(string error)
            => new()
            {
                IsSuccess = false,
                Error = error
            };
    }
}
