using FluentValidation;
using northguan_nsa_vue_app.Server.Exceptions;

namespace northguan_nsa_vue_app.Server.Extensions
{
    /// <summary>
    /// FluentValidation 擴展方法
    /// </summary>
    public static class FluentValidationExtensions
    {
        /// <summary>
        /// 將 FluentValidation 結果轉換為自定義 ValidationException
        /// </summary>
        public static northguan_nsa_vue_app.Server.Exceptions.ValidationException ToValidationException(this FluentValidation.Results.ValidationResult validationResult)
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

            return new northguan_nsa_vue_app.Server.Exceptions.ValidationException(validationErrors);
        }

        /// <summary>
        /// 標準化欄位名稱，保持原始英文名稱
        /// </summary>
        private static string NormalizeFieldName(string fieldName)
        {
            // 處理巢狀屬性名稱 (例如: "Parameters.Page" -> "Page")
            return fieldName.Contains('.') ? fieldName.Split('.').Last() : fieldName;
        }
    }
}