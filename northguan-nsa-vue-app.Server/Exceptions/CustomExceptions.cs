using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Exceptions
{
    /// <summary>
    /// 基礎業務異常類別
    /// </summary>
    public abstract class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        protected BusinessException(string errorCode, string message, int statusCode = 400) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        protected BusinessException(string errorCode, string message, Exception innerException, int statusCode = 400) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// 認證相關異常
    /// </summary>
    public class AuthenticationException : BusinessException
    {
        public AuthenticationException(string message) 
            : base(ErrorCodes.INVALID_CREDENTIALS, message, 401)
        {
        }

        public AuthenticationException(string errorCode, string message) 
            : base(errorCode, message, 401)
        {
        }
    }

    /// <summary>
    /// 授權相關異常
    /// </summary>
    public class AuthorizationException : BusinessException
    {
        public AuthorizationException(string message) 
            : base(ErrorCodes.INSUFFICIENT_PERMISSIONS, message, 403)
        {
        }
    }

    /// <summary>
    /// 資源未找到異常
    /// </summary>
    public class ResourceNotFoundException : BusinessException
    {
        public ResourceNotFoundException(string resourceName, string identifier) 
            : base(ErrorCodes.RESOURCE_NOT_FOUND, $"{resourceName} '{identifier}' 未找到", 404)
        {
        }

        public ResourceNotFoundException(string message) 
            : base(ErrorCodes.RESOURCE_NOT_FOUND, message, 404)
        {
        }
    }

    /// <summary>
    /// 資源已存在異常
    /// </summary>
    public class ResourceAlreadyExistsException : BusinessException
    {
        public ResourceAlreadyExistsException(string resourceName, string identifier) 
            : base(ErrorCodes.RESOURCE_ALREADY_EXISTS, $"{resourceName} '{identifier}' 已存在", 409)
        {
        }

        public ResourceAlreadyExistsException(string message) 
            : base(ErrorCodes.RESOURCE_ALREADY_EXISTS, message, 409)
        {
        }
    }

    /// <summary>
    /// 驗證失敗異常
    /// </summary>
    public class ValidationException : BusinessException
    {
        public Dictionary<string, List<string>> ValidationErrors { get; }

        public ValidationException(string message) 
            : base(ErrorCodes.VALIDATION_FAILED, message, 400)
        {
            ValidationErrors = new Dictionary<string, List<string>>();
        }

        public ValidationException(Dictionary<string, List<string>> validationErrors) 
            : base(ErrorCodes.VALIDATION_FAILED, "驗證失敗", 400)
        {
            ValidationErrors = validationErrors;
        }

        public ValidationException(string field, string error) 
            : base(ErrorCodes.VALIDATION_FAILED, "驗證失敗", 400)
        {
            ValidationErrors = new Dictionary<string, List<string>>
            {
                { field, new List<string> { error } }
            };
        }
    }

    /// <summary>
    /// 業務規則違反異常
    /// </summary>
    public class BusinessRuleViolationException : BusinessException
    {
        public BusinessRuleViolationException(string message) 
            : base(ErrorCodes.BUSINESS_RULE_VIOLATION, message, 400)
        {
        }
    }

    /// <summary>
    /// 操作不允許異常
    /// </summary>
    public class OperationNotAllowedException : BusinessException
    {
        public OperationNotAllowedException(string message) 
            : base(ErrorCodes.OPERATION_NOT_ALLOWED, message, 403)
        {
        }
    }

    /// <summary>
    /// 內部伺服器錯誤異常
    /// </summary>
    public class InternalServerErrorException : BusinessException
    {
        public InternalServerErrorException(string message, Exception? innerException = null) 
            : base(ErrorCodes.INTERNAL_SERVER_ERROR, message, innerException, 500)
        {
        }
    }
}