namespace VisaSponsorshipScout.API.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; private set; }
        public T Result { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static ApiResponse<T> Ok(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Result = data
            };
        }

        public static ApiResponse<T> Fail(string errorMessage)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class ApiResponse
    {
        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static ApiResponse Ok()
        {
            return new ApiResponse
            {
                Success = true,
            };
        }

        public static ApiResponse Fail(string errorMessage)
        {
            return new ApiResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public ApiResponse<T> WithData<T>(T data)
        {
            return Success
                ? ApiResponse<T>.Ok(data)
                : ApiResponse<T>.Fail(ErrorMessage);
        }
    }
}
