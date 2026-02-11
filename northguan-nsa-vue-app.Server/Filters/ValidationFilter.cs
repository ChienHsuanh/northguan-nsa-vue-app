using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;
using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Filters
{
    /// <summary>
    /// 統一驗證錯誤處理過濾器
    /// 自動處理 ModelState 和 FluentValidation 錯誤，將所有驗證錯誤轉換為統一格式
    /// </summary>
    public class ValidationFilter : ActionFilterAttribute
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ValidationFilter> _logger;

        public ValidationFilter(
            IServiceProvider serviceProvider,
            ILogger<ValidationFilter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 首先檢查 ModelState 錯誤
            if (!context.ModelState.IsValid)
            {
                var modelStateErrors = ConvertModelStateErrors(context.ModelState);

                _logger.LogWarning("ModelState validation failed: {Errors}",
                    string.Join(", ", modelStateErrors.SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))));

                context.Result = CreateValidationErrorResponse(modelStateErrors, context.HttpContext.Request.Path);
                return;
            }

            // 然後檢查 FluentValidation 錯誤
            foreach (var parameter in context.ActionDescriptor.Parameters)
            {
                if (context.ActionArguments.TryGetValue(parameter.Name, out var argumentValue) && argumentValue != null)
                {
                    var parameterType = parameter.ParameterType;

                    // 尋找對應的驗證器
                    var validatorType = typeof(IValidator<>).MakeGenericType(parameterType);
                    var validator = _serviceProvider.GetService(validatorType) as IValidator;

                    if (validator != null)
                    {
                        // 執行驗證
                        var validationContext = new ValidationContext<object>(argumentValue);
                        var validationResult = await validator.ValidateAsync(validationContext);

                        if (!validationResult.IsValid)
                        {
                            // 轉換驗證錯誤為統一格式
                            var validationErrors = ConvertFluentValidationErrors(validationResult);

                            // 記錄驗證錯誤
                            _logger.LogWarning("FluentValidation failed for {ParameterType}: {Errors}",
                                parameterType.Name,
                                string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

                            // 創建統一的錯誤響應
                            context.Result = CreateValidationErrorResponse(validationErrors, context.HttpContext.Request.Path);
                            return;
                        }
                    }
                }
            }

            await next();
        }

        /// <summary>
        /// 創建統一的驗證錯誤響應
        /// </summary>
        private BadRequestObjectResult CreateValidationErrorResponse(
            Dictionary<string, List<string>> validationErrors,
            string path)
        {
            var errorResponse = new ApiErrorResponse
            {
                ErrorCode = ErrorCodes.VALIDATION_FAILED,
                Message = "驗證失敗",
                ValidationErrors = validationErrors,
                StatusCode = 400,
                Path = path,
                Timestamp = DateTime.Now
            };

            return new BadRequestObjectResult(errorResponse);
        }

        /// <summary>
        /// 將 FluentValidation 錯誤轉換為統一格式
        /// </summary>
        private Dictionary<string, List<string>> ConvertFluentValidationErrors(
            FluentValidation.Results.ValidationResult validationResult)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            foreach (var error in validationResult.Errors)
            {
                var fieldName = NormalizeFieldName(error.PropertyName);

                if (!validationErrors.ContainsKey(fieldName))
                {
                    validationErrors[fieldName] = new List<string>();
                }

                validationErrors[fieldName].Add(error.ErrorMessage);
            }

            return validationErrors;
        }

        /// <summary>
        /// 將 ModelState 錯誤轉換為統一格式
        /// </summary>
        private Dictionary<string, List<string>> ConvertModelStateErrors(
            Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            foreach (var modelError in modelState)
            {
                // 排除 request field
                if (ShouldExcludeField(modelError.Key))
                {
                    continue;
                }

                var errors = new List<string>();
                foreach (var error in modelError.Value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }

                if (errors.Any())
                {
                    var fieldName = NormalizeFieldName(modelError.Key);
                    validationErrors.Add(fieldName, errors);
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// 標準化欄位名稱，保持原始英文名稱
        /// </summary>
        private string NormalizeFieldName(string fieldName)
        {
            // 處理巢狀屬性名稱 (例如: "Parameters.Page" -> "Page")
            return fieldName.Contains('.') ? fieldName.Split('.').Last() : fieldName;
        }

        /// <summary>
        /// 判斷是否應該排除某個欄位
        /// </summary>
        private bool ShouldExcludeField(string fieldName)
        {
            // 排除一些不需要顯示給用戶的內部欄位
            var excludeFields = new[]
            {
                "request",
                "$",
                "__RequestVerificationToken"
            };

            return excludeFields.Any(exclude =>
                fieldName.Equals(exclude, StringComparison.OrdinalIgnoreCase) ||
                fieldName.StartsWith(exclude + ".", StringComparison.OrdinalIgnoreCase));
        }

    }
}