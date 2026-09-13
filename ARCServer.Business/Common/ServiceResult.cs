namespace ARCServer.Business.Common
{
    public enum ServiceErrorType
    {
        None = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
    }

    public class ServiceResult<T>
    {
        public bool Succeeded { get; init; }

        public T? Data { get; init; }

        public ServiceErrorType ErrorType { get; init; }

        public Dictionary<string, string[]> Errors { get; init; } = new();

        public bool IsNotFound => ErrorType == ServiceErrorType.NotFound;

        public bool IsValidationError =>
            ErrorType is ServiceErrorType.Validation or ServiceErrorType.Conflict;

        public static ServiceResult<T> Success(T data) => new()
        {
            Succeeded = true,
            Data = data,
            ErrorType = ServiceErrorType.None,
        };

        public static ServiceResult<T> Failure(
            Dictionary<string, string[]> errors,
            ServiceErrorType errorType = ServiceErrorType.Validation) => new()
        {
            Succeeded = false,
            Errors = errors,
            ErrorType = errorType,
        };

        public static ServiceResult<T> Failure(
            string key,
            string message,
            ServiceErrorType errorType = ServiceErrorType.Validation) =>
            Failure(new Dictionary<string, string[]> { [key] = [message] }, errorType);

        public static ServiceResult<T> NotFound(string key, string message) =>
            Failure(key, message, ServiceErrorType.NotFound);
    }
}
