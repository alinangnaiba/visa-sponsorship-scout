namespace VisaSponsorshipScout.API.Common
{
    public class ApiResult<T>
    {
        public bool Success { get; private set; }

        public T Data { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static ApiResult<T> Ok(T data)
        {
            return new ApiResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ApiResult<T> Fail(string errorMessage)
        {
            return new ApiResult<T>
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class ApiResult
    {
        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static ApiResult Ok()
        {
            return new ApiResult
            {
                Success = true,
            };
        }

        public static ApiResult Fail(string errorMessage)
        {
            return new ApiResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public ApiResult<T> WithData<T>(T data)
        {
            return Success
                ? ApiResult<T>.Ok(data)
                : ApiResult<T>.Fail(ErrorMessage);
        }
    }
}
