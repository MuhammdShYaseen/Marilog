using Marilog.Contracts.Common;
using System.Net;

namespace Marilog.Client.ErrorUniform
{
    public sealed class ApiException : Exception
    {
        public ApiErrorResponse? ApiError { get;}
        public string? ApiErrorCode { get;}
        public string? ErrorMessage { get;}

        public ApiException(ApiErrorResponse? apiError, string? apiErrorCode, string? errorMessage)
        {
            ApiError = apiError;
            ApiErrorCode = apiErrorCode;
            ErrorMessage = errorMessage;
        }
    }
}
