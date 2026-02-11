using FluentValidation;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;

namespace northguan_nsa_vue_app.Server.Validators
{
    /// <summary>
    /// 記錄查詢參數驗證器基類
    /// </summary>
    /// <typeparam name="T">查詢參數類型</typeparam>
    public abstract class RecordQueryParametersValidator<T> : AbstractValidator<T>
        where T : RecordQueryParameters
    {
        protected readonly RecordServiceOptions _options;

        protected RecordQueryParametersValidator(IOptions<RecordServiceOptions> options)
        {
            _options = options.Value;
            SetupCommonRules();
        }

        /// <summary>
        /// 設置通用驗證規則
        /// </summary>
        protected virtual void SetupCommonRules()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("頁碼必須大於0");

            RuleFor(x => x.Size)
                .GreaterThan(0)
                .WithMessage("頁面大小必須大於0")
                .LessThanOrEqualTo(_options.MaxPageSize)
                .WithMessage($"頁面大小不能超過{_options.MaxPageSize}");

            RuleFor(x => x.Keyword)
                .MaximumLength(100)
                .WithMessage("關鍵字長度不能超過100個字符");

            RuleFor(x => x.SortBy)
                .Must(BeValidSortField)
                .WithMessage("無效的排序字段")
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.SortOrder)
                .Must(x => string.IsNullOrEmpty(x) || x.ToLower() == "asc" || x.ToLower() == "desc")
                .WithMessage("排序順序只能是 'asc' 或 'desc'");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("開始日期不能晚於結束日期")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.EndDate)
                .LessThanOrEqualTo(DateTime.Now.AddDays(1))
                .WithMessage("結束日期不能超過明天");

            RuleFor(x => x.StationIds)
                .Must(x => x == null || x.Count <= 50)
                .WithMessage("分站ID數量不能超過50個");

            RuleFor(x => x.DeviceSerials)
                .Must(x => x == null || x.Count <= 100)
                .WithMessage("設備序號數量不能超過100個");
        }

        /// <summary>
        /// 驗證排序字段是否有效
        /// </summary>
        protected virtual bool BeValidSortField(string sortField)
        {
            if (string.IsNullOrEmpty(sortField))
                return false;

            var validFields = GetValidSortFields();
            return validFields.Contains(sortField.ToLower());
        }

        /// <summary>
        /// 獲取有效的排序字段列表
        /// 自動從響應DTO和實體模型中提取屬性名稱
        /// </summary>
        protected virtual HashSet<string> GetValidSortFields()
        {
            var validFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 添加通用的時間字段
            validFields.Add("timestamp");
            validFields.Add("time");

            // 從響應DTO中提取屬性名稱
            var responseType = GetResponseType();
            if (responseType != null)
            {
                AddPropertiesFromType(validFields, responseType);
            }

            // 從實體模型中提取屬性名稱
            var entityType = GetEntityType();
            if (entityType != null)
            {
                AddPropertiesFromType(validFields, entityType);
            }

            // 添加導航屬性的字段
            AddNavigationProperties(validFields);

            return validFields;
        }

        /// <summary>
        /// 從類型中添加屬性名稱到有效字段集合
        /// </summary>
        private void AddPropertiesFromType(HashSet<string> validFields, Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                // 跳過標記為 NotMapped 的屬性
                if (property.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;

                // 跳過複雜類型（導航屬性等）
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    continue;

                validFields.Add(property.Name.ToLower());
            }
        }

        /// <summary>
        /// 添加導航屬性相關的字段
        /// </summary>
        protected virtual void AddNavigationProperties(HashSet<string> validFields)
        {
            // 設備相關字段
            validFields.Add("devicename");
            validFields.Add("deviceserial");
            
            // 分站相關字段
            validFields.Add("stationname");
            
            // 其他常用別名
            validFields.Add("count");
            validFields.Add("peoplecount");
            validFields.Add("parkednum");
            validFields.Add("occupiedspaces");
            validFields.Add("event");
            validFields.Add("eventtype");
        }

        /// <summary>
        /// 獲取響應DTO類型
        /// </summary>
        protected abstract Type GetResponseType();

        /// <summary>
        /// 獲取實體模型類型
        /// </summary>
        protected abstract Type GetEntityType();
    }

    /// <summary>
    /// 電子圍籬記錄查詢參數驗證器
    /// </summary>
    public class FenceRecordQueryParametersValidator : RecordQueryParametersValidator<FenceRecordQueryParameters>
    {
        public FenceRecordQueryParametersValidator(IOptions<RecordServiceOptions> options) : base(options)
        {
            SetupFenceSpecificRules();
        }

        protected override Type GetResponseType() => typeof(FenceRecordListResponse);
        protected override Type GetEntityType() => typeof(Models.FenceRecord);

        private void SetupFenceSpecificRules()
        {
            RuleFor(x => x.EventTypes)
                .Must(x => x == null || x.All(et => Enum.TryParse<Models.FenceEventType>(et, out _)))
                .WithMessage("包含無效的事件類型")
                .When(x => x.EventTypes != null && x.EventTypes.Count > 0);

            RuleFor(x => x.EventTypes)
                .Must(x => x == null || x.Count <= 10)
                .WithMessage("事件類型數量不能超過10個");
        }
    }

    /// <summary>
    /// 人流記錄查詢參數驗證器
    /// </summary>
    public class CrowdRecordQueryParametersValidator : RecordQueryParametersValidator<CrowdRecordQueryParameters>
    {
        public CrowdRecordQueryParametersValidator(IOptions<RecordServiceOptions> options) : base(options)
        {
            SetupCrowdSpecificRules();
        }

        protected override Type GetResponseType() => typeof(CrowdRecordListResponse);
        protected override Type GetEntityType() => typeof(Models.CrowdRecord);

        private void SetupCrowdSpecificRules()
        {
            RuleFor(x => x.MinPeopleCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("最小人數不能小於0")
                .When(x => x.MinPeopleCount.HasValue);

            RuleFor(x => x.MaxPeopleCount)
                .GreaterThanOrEqualTo(x => x.MinPeopleCount)
                .WithMessage("最大人數不能小於最小人數")
                .When(x => x.MaxPeopleCount.HasValue && x.MinPeopleCount.HasValue);

            RuleFor(x => x.MinDensity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("最小密度不能小於0")
                .When(x => x.MinDensity.HasValue);

            RuleFor(x => x.MaxDensity)
                .GreaterThanOrEqualTo(x => x.MinDensity)
                .WithMessage("最大密度不能小於最小密度")
                .When(x => x.MaxDensity.HasValue && x.MinDensity.HasValue);

            RuleFor(x => x.MinArea)
                .GreaterThanOrEqualTo(0)
                .WithMessage("最小面積不能小於0")
                .When(x => x.MinArea.HasValue);

            RuleFor(x => x.MaxArea)
                .GreaterThanOrEqualTo(x => x.MinArea)
                .WithMessage("最大面積不能小於最小面積")
                .When(x => x.MaxArea.HasValue && x.MinArea.HasValue);
        }
    }

    /// <summary>
    /// 停車記錄查詢參數驗證器
    /// </summary>
    public class ParkingRecordQueryParametersValidator : RecordQueryParametersValidator<ParkingRecordQueryParameters>
    {
        public ParkingRecordQueryParametersValidator(IOptions<RecordServiceOptions> options) : base(options)
        {
            SetupParkingSpecificRules();
        }

        protected override Type GetResponseType() => typeof(ParkingRecordListResponse);
        protected override Type GetEntityType() => typeof(Models.ParkingRecord);

        private void SetupParkingSpecificRules()
        {
            RuleFor(x => x.MinTotalSpaces)
                .GreaterThanOrEqualTo(0)
                .WithMessage("最小總車位數不能小於0")
                .When(x => x.MinTotalSpaces.HasValue);

            RuleFor(x => x.MaxTotalSpaces)
                .GreaterThanOrEqualTo(x => x.MinTotalSpaces)
                .WithMessage("最大總車位數不能小於最小總車位數")
                .When(x => x.MaxTotalSpaces.HasValue && x.MinTotalSpaces.HasValue);

            RuleFor(x => x.MinOccupancyRate)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("最小使用率必須在0-100之間")
                .When(x => x.MinOccupancyRate.HasValue);

            RuleFor(x => x.MaxOccupancyRate)
                .GreaterThanOrEqualTo(x => x.MinOccupancyRate)
                .LessThanOrEqualTo(100)
                .WithMessage("最大使用率必須在0-100之間且不能小於最小使用率")
                .When(x => x.MaxOccupancyRate.HasValue);
        }
    }

    /// <summary>
    /// 車流記錄查詢參數驗證器
    /// </summary>
    public class TrafficRecordQueryParametersValidator : RecordQueryParametersValidator<TrafficRecordQueryParameters>
    {
        public TrafficRecordQueryParametersValidator(IOptions<RecordServiceOptions> options) : base(options)
        {
            SetupTrafficSpecificRules();
        }

        protected override Type GetResponseType() => typeof(TrafficRecordListResponse);
        protected override Type GetEntityType() => typeof(Models.TrafficRecord);

        private void SetupTrafficSpecificRules()
        {
            RuleFor(x => x.MinVehicleCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("最小車輛數不能小於0")
                .When(x => x.MinVehicleCount.HasValue);

            RuleFor(x => x.MaxVehicleCount)
                .GreaterThanOrEqualTo(x => x.MinVehicleCount)
                .WithMessage("最大車輛數不能小於最小車輛數")
                .When(x => x.MaxVehicleCount.HasValue && x.MinVehicleCount.HasValue);

            RuleFor(x => x.MinAverageSpeed)
                .GreaterThanOrEqualTo(0)
                .WithMessage("最小平均速度不能小於0")
                .When(x => x.MinAverageSpeed.HasValue);

            RuleFor(x => x.MaxAverageSpeed)
                .GreaterThanOrEqualTo(x => x.MinAverageSpeed)
                .WithMessage("最大平均速度不能小於最小平均速度")
                .When(x => x.MaxAverageSpeed.HasValue && x.MinAverageSpeed.HasValue);

            RuleFor(x => x.SpeedStatuses)
                .Must(x => x == null || x.All(status => IsValidSpeedStatus(status)))
                .WithMessage("包含無效的速度狀態")
                .When(x => x.SpeedStatuses != null && x.SpeedStatuses.Count > 0);

            RuleFor(x => x.Cities)
                .Must(x => x == null || x.Count <= 20)
                .WithMessage("城市數量不能超過20個");
        }

        private bool IsValidSpeedStatus(string status)
        {
            var validStatuses = new[] { "無速限", "超速嚴重", "輕微超速", "正常", "緩慢", "壅塞" };
            return validStatuses.Contains(status);
        }
    }
}