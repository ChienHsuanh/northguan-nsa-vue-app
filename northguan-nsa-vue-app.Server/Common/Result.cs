namespace northguan_nsa_vue_app.Server.Common
{
    /// <summary>
    /// 通用結果包裝器
    /// </summary>
    /// <typeparam name="T">數據類型</typeparam>
    public class Result<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Code { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public static Result<T> SuccessResult(T data, string? message = null)
        {
            return new Result<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static Result<T> FailureResult(string message, string? code = null)
        {
            return new Result<T>
            {
                Success = false,
                Message = message,
                Code = code
            };
        }

        public static Result<T> FailureResult(List<string> errors, string? message = null, string? code = null)
        {
            return new Result<T>
            {
                Success = false,
                Errors = errors,
                Message = message,
                Code = code
            };
        }
    }

    /// <summary>
    /// 無數據的結果包裝器
    /// </summary>
    public class Result
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Code { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public static Result SuccessResult(string? message = null)
        {
            return new Result
            {
                Success = true,
                Message = message
            };
        }

        public static Result FailureResult(string message, string? code = null)
        {
            return new Result
            {
                Success = false,
                Message = message,
                Code = code
            };
        }

        public static Result FailureResult(List<string> errors, string? message = null, string? code = null)
        {
            return new Result
            {
                Success = false,
                Errors = errors,
                Message = message,
                Code = code
            };
        }
    }
}