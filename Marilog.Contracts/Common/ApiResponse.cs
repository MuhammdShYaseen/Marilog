namespace Marilog.Contracts.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public IReadOnlyList<string>? Errors { get; init; }

        public static ApiResponse<T> Ok(T data, string? message = null)
            => new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Ok(string message)
            => new() { Success = true, Message = message };

        public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null)
            => new() { Success = false, Message = message, Errors = errors?.ToList() };
    }
}
