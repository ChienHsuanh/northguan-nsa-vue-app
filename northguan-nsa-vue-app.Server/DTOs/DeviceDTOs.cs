using System.ComponentModel.DataAnnotations;
using northguan_nsa_vue_app.Server.Resources;
using northguan_nsa_vue_app.Server.Attributes;

namespace northguan_nsa_vue_app.Server.DTOs
{
    public class DeviceListRequest
    {
        public string Type { get; set; } = "";
        
        public string Keyword { get; set; } = "";
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
    }

    public class DeviceListResponse
    {
        public required string Type { get; set; }
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public int StationID { get; set; }
        public required string Serial { get; set; }
        public required string StationName { get; set; }
        
        // Device status fields
        public string? Status { get; set; }
        public string? LatestOnlineTime { get; set; }
        public string? UpdatedAt { get; set; }

        // Crowd specific
        public int Area { get; set; }
        public string? VideoUrl { get; set; }
        public string? ApiUrl { get; set; }

        // Parking specific
        public int NumberOfParking { get; set; }

        // Traffic specific
        public string? City { get; set; }
        public string? ETagNumber { get; set; }
        public int SpeedLimit { get; set; }

        // Fence specific
        public string? ObservingTimeStart { get; set; }
        public string? ObservingTimeEnd { get; set; }
        public string? Zones { get; set; }
        public string? CameraConfig { get; set; }
    }

    [DeviceTypeValidation]
    public class CreateDeviceRequest
    {
        [Required(ErrorMessage = ValidationMessages.Specific.TypeRequired)]
        public required string Type { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.StationRequired)]
        [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Specific.StationRequired)]
        public int StationID { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.NameRequired)]
        [StringLength(100, ErrorMessage = ValidationMessages.StringLengthMax)]
        public required string Name { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.CoordinateRequired)]
        [Range(-180, 180, ErrorMessage = ValidationMessages.Specific.CoordinateRange)]
        public decimal Lng { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.CoordinateRequired)]
        [Range(-90, 90, ErrorMessage = ValidationMessages.Specific.CoordinateRange)]
        public decimal Lat { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.SerialRequired)]
        [StringLength(50, ErrorMessage = ValidationMessages.StringLengthMax)]
        public required string Serial { get; set; }

        // Type-specific properties - 設為可選，因為不是所有設備類型都需要這些字段
        public int? Area { get; set; }
        public string? VideoUrl { get; set; }
        public string? ApiUrl { get; set; }
        public int? NumberOfParking { get; set; }
        public string? City { get; set; }
        public string? ETagNumber { get; set; }
        public int? SpeedLimit { get; set; }
        public string? ObservingTimeStart { get; set; }
        public string? ObservingTimeEnd { get; set; }
        public string? CameraConfig { get; set; }
    }

    [DeviceTypeValidation]
    public class UpdateDeviceRequest
    {
        [Required(ErrorMessage = ValidationMessages.Specific.TypeRequired)]
        public required string Type { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.NameRequired)]
        [StringLength(100, ErrorMessage = ValidationMessages.StringLengthMax)]
        public required string Name { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.CoordinateRequired)]
        [Range(-180, 180, ErrorMessage = ValidationMessages.Specific.CoordinateRange)]
        public decimal Lng { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.CoordinateRequired)]
        [Range(-90, 90, ErrorMessage = ValidationMessages.Specific.CoordinateRange)]
        public decimal Lat { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.SerialRequired)]
        [StringLength(50, ErrorMessage = ValidationMessages.StringLengthMax)]
        public required string Serial { get; set; }

        // Type-specific properties - 設為可選，因為不是所有設備類型都需要這些字段
        public int? Area { get; set; }
        public string? VideoUrl { get; set; }
        public string? ApiUrl { get; set; }
        public int? NumberOfParking { get; set; }
        public string? City { get; set; }
        public string? ETagNumber { get; set; }
        public int? SpeedLimit { get; set; }
        public string? ObservingTimeStart { get; set; }
        public string? ObservingTimeEnd { get; set; }
        public string? CameraConfig { get; set; }
    }

    public class DeviceStatusResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Serial { get; set; }
        public required string Status { get; set; }
        public string? LatestOnlineTime { get; set; }
        public required string StationName { get; set; }
        public required string Type { get; set; }
    }

    public class DeviceStatusLogResponse
    {
        public int Id { get; set; }
        public required string DeviceType { get; set; }
        public required string DeviceSerial { get; set; }
        public required string Status { get; set; }
        public required string Time { get; set; }
        public required string DeviceName { get; set; }
        public required string StationName { get; set; }
    }
}