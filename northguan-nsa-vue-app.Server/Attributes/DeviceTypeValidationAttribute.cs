using System.ComponentModel.DataAnnotations;
using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Attributes
{
    /// <summary>
    /// 設備類型特定字段驗證屬性
    /// </summary>
    public class DeviceTypeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is CreateDeviceRequest createRequest)
            {
                return ValidateCreateRequest(createRequest);
            }
            
            if (value is UpdateDeviceRequest updateRequest)
            {
                return ValidateUpdateRequest(updateRequest);
            }
            
            return ValidationResult.Success;
        }

        private ValidationResult? ValidateCreateRequest(CreateDeviceRequest request)
        {
            switch (request.Type?.ToLower())
            {
                case "crowd":
                    if (!request.Area.HasValue || request.Area.Value <= 0)
                    {
                        return new ValidationResult("人流設備的區域面積為必填且必須大於0", new[] { "Area" });
                    }
                    break;
                    
                case "parking":
                    if (!request.NumberOfParking.HasValue || request.NumberOfParking.Value <= 0)
                    {
                        return new ValidationResult("停車設備的停車位數量為必填且必須大於0", new[] { "NumberOfParking" });
                    }
                    break;
                    
                case "traffic":
                    if (!request.SpeedLimit.HasValue || request.SpeedLimit.Value <= 0)
                    {
                        return new ValidationResult("交通設備的速限為必填且必須大於0", new[] { "SpeedLimit" });
                    }
                    break;
                    
                case "fence":
                    if (string.IsNullOrEmpty(request.ObservingTimeStart) || string.IsNullOrEmpty(request.ObservingTimeEnd))
                    {
                        return new ValidationResult("圍籬設備的觀測時間為必填", new[] { "ObservingTimeStart", "ObservingTimeEnd" });
                    }
                    break;
                    
                case "highresolution":
                    if (string.IsNullOrEmpty(request.VideoUrl))
                    {
                        return new ValidationResult("高解析度設備的影片網址為必填", new[] { "VideoUrl" });
                    }
                    break;
            }
            
            return ValidationResult.Success;
        }

        private ValidationResult? ValidateUpdateRequest(UpdateDeviceRequest request)
        {
            switch (request.Type?.ToLower())
            {
                case "crowd":
                    if (request.Area.HasValue && request.Area.Value <= 0)
                    {
                        return new ValidationResult("人流設備的區域面積必須大於0", new[] { "Area" });
                    }
                    break;
                    
                case "parking":
                    if (request.NumberOfParking.HasValue && request.NumberOfParking.Value <= 0)
                    {
                        return new ValidationResult("停車設備的停車位數量必須大於0", new[] { "NumberOfParking" });
                    }
                    break;
                    
                case "traffic":
                    if (request.SpeedLimit.HasValue && request.SpeedLimit.Value <= 0)
                    {
                        return new ValidationResult("交通設備的速限必須大於0", new[] { "SpeedLimit" });
                    }
                    break;
            }
            
            return ValidationResult.Success;
        }
    }
}