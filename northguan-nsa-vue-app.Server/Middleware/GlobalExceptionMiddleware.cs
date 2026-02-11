using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Exceptions;

namespace northguan_nsa_vue_app.Server.Middleware
{
    /// <summary>
    /// 全域異常處理中介軟體
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = CreateErrorResponse(context, exception);

            context.Response.StatusCode = response.StatusCode;
            context.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        private ApiErrorResponse CreateErrorResponse(HttpContext context, Exception exception)
        {
            var response = new ApiErrorResponse
            {
                ErrorCode = string.Empty, // 將在下面的 switch 中設定
                Message = string.Empty,   // 將在下面的 switch 中設定
                Path = context.Request.Path,
                Timestamp = DateTime.Now
            };

            switch (exception)
            {
                case BusinessException businessEx:
                    response.ErrorCode = businessEx.ErrorCode;
                    response.Message = businessEx.Message;
                    response.StatusCode = businessEx.StatusCode;

                    if (businessEx is ValidationException validationEx)
                    {
                        response.ValidationErrors = validationEx.ValidationErrors;
                    }
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    response.ErrorCode = ErrorCodes.UNAUTHORIZED;
                    response.Message = unauthorizedEx.Message;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case ArgumentException argEx:
                    response.ErrorCode = ErrorCodes.INVALID_INPUT;
                    response.Message = argEx.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException invalidOpEx:
                    response.ErrorCode = ErrorCodes.OPERATION_NOT_ALLOWED;
                    response.Message = invalidOpEx.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case NotImplementedException notImplEx:
                    response.ErrorCode = ErrorCodes.OPERATION_NOT_ALLOWED;
                    response.Message = "此功能尚未實作";
                    response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    break;

                default:
                    response.ErrorCode = ErrorCodes.INTERNAL_SERVER_ERROR;
                    response.Message = "系統發生未預期的錯誤";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // 在開發環境中提供詳細的錯誤資訊
            if (_environment.IsDevelopment())
            {
                response.Details = exception.ToString();
            }

            return response;
        }
    }

    /// <summary>
    /// 中介軟體擴展方法
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}