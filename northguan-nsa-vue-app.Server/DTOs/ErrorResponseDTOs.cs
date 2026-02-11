using System.ComponentModel;
using Newtonsoft.Json;

namespace northguan_nsa_vue_app.Server.DTOs
{
    /// <summary>
    /// 統一的 API 錯誤回應格式
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// 錯誤代碼，用於前端程式化處理
        /// </summary>
        /// <example>INVALID_CREDENTIALS</example>
        [JsonProperty("errorCode")]
        public required string ErrorCode { get; set; }

        /// <summary>
        /// 錯誤訊息，用於顯示給使用者
        /// </summary>
        /// <example>帳號或密碼錯誤</example>
        [JsonProperty("message")]
        public required string Message { get; set; }

        /// <summary>
        /// 詳細錯誤資訊（僅開發環境）
        /// </summary>
        /// <example>System.UnauthorizedAccessException: Invalid credentials</example>
        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
        public string? Details { get; set; }

        /// <summary>
        /// 欄位驗證錯誤
        /// </summary>
        /// <example>{"使用者名稱": ["使用者名稱為必填欄位"], "密碼": ["密碼長度必須介於 6 到 100 個字元之間"]}</example>
        [JsonProperty("validationErrors", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<string>>? ValidationErrors { get; set; }

        /// <summary>
        /// 錯誤發生的時間戳
        /// </summary>
        /// <example>2024-08-17T00:30:00Z</example>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// 請求的路徑
        /// </summary>
        /// <example>/api/login</example>
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string? Path { get; set; }

        /// <summary>
        /// HTTP 狀態碼
        /// </summary>
        /// <example>401</example>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// 預定義的錯誤代碼常數
    /// </summary>
    /// <summary>
    /// 簡單的 API 錯誤回應格式
    /// </summary>
    public class SimpleErrorResponse
    {
        /// <summary>
        /// 錯誤類型或狀態
        /// </summary>
        /// <example>Bad Request</example>
        [JsonProperty("error")]
        public required string Error { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        /// <example>time 欄位是必填的</example>
        [JsonProperty("msg")]
        public required string Msg { get; set; }
    }

    public static class ErrorCodes
    {
        // 認證相關錯誤
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
        public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
        public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
        public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";

        // 資源相關錯誤
        public const string RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND";
        public const string RESOURCE_ALREADY_EXISTS = "RESOURCE_ALREADY_EXISTS";
        public const string RESOURCE_IN_USE = "RESOURCE_IN_USE";

        // 驗證相關錯誤
        public const string VALIDATION_FAILED = "VALIDATION_FAILED";
        public const string INVALID_INPUT = "INVALID_INPUT";
        public const string MISSING_REQUIRED_FIELD = "MISSING_REQUIRED_FIELD";

        // 業務邏輯錯誤
        public const string BUSINESS_RULE_VIOLATION = "BUSINESS_RULE_VIOLATION";
        public const string OPERATION_NOT_ALLOWED = "OPERATION_NOT_ALLOWED";

        // 系統錯誤
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string DATABASE_ERROR = "DATABASE_ERROR";
        public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";
    }
}