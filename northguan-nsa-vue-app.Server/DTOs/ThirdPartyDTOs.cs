using System.Text.Json.Serialization;

namespace northguan_nsa_vue_app.Server.DTOs
{
    // Third Party API Response DTOs - matching old project format
    
    // Station List - returns direct array
    public class ThirdPartyStationInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("lat")]
        public decimal Lat { get; set; }
        [JsonPropertyName("lng")]
        public decimal Lng { get; set; }
    }

    // Fence Device List - returns direct array  
    public class ThirdPartyFenceDeviceInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("station_id")]
        public int StationId { get; set; }
    }

    // Crowd Device List - returns direct array
    public class ThirdPartyCrowdDeviceInfo
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; } // serial as id
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("station_id")]
        public int StationId { get; set; }
        [JsonPropertyName("lat")]
        public decimal Lat { get; set; }
        [JsonPropertyName("lng")]
        public decimal Lng { get; set; }
        [JsonPropertyName("area")]
        public int? Area { get; set; }
        [JsonPropertyName("count")]
        public int? Count { get; set; }
        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }

    // Parking Device List - returns direct array
    public class ThirdPartyParkingDeviceInfo
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; } // serial as id
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("lat")]
        public decimal Lat { get; set; }
        [JsonPropertyName("lng")]
        public decimal Lng { get; set; }
        [JsonPropertyName("station_id")]
        public int StationId { get; set; }
        [JsonPropertyName("number_of_spaces")]
        public int NumberOfSpaces { get; set; }
        [JsonPropertyName("parked_num")]
        public int ParkedNum { get; set; }
        [JsonPropertyName("total_in")]
        public int TotalIn { get; set; }
        [JsonPropertyName("total_out")]
        public int TotalOut { get; set; }
        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }

    // Traffic Device List - returns direct array
    public class ThirdPartyTrafficDeviceInfo
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; } // serial as id
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("lat")]
        public decimal Lat { get; set; }
        [JsonPropertyName("lng")]
        public decimal Lng { get; set; }
        [JsonPropertyName("station_id")]
        public int StationId { get; set; }
        [JsonPropertyName("etag_pair_id")]
        public string? EtagPairId { get; set; }
        [JsonPropertyName("speed_limit")]
        public int? SpeedLimit { get; set; }
        [JsonPropertyName("travel_time")]
        public decimal? TravelTime { get; set; }
        [JsonPropertyName("space_mean_speed")]
        public decimal? SpaceMeanSpeed { get; set; }
        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }

    // Third-party API DTOs
    public class CreateFenceRecordRequest
    {
        [JsonPropertyName("camera_id")]
        public required string DeviceSerial { get; set; }
        [JsonPropertyName("event_type")]
        public int EventType { get; set; }
        [JsonPropertyName("time")]
        public required string Time { get; set; }
        [JsonPropertyName("snapshot")]
        public string? Snapshot { get; set; }
    }

    public class UpdateFenceHeartbeatRequest
    {
        [JsonPropertyName("camera_id")]
        public required string DeviceSerial { get; set; }
        [JsonPropertyName("time")]
        public required string Time { get; set; }
    }

    public class CreateCrowdRecordRequest
    {
        [JsonPropertyName("camera_id")]
        public required string DeviceSerial { get; set; }
        [JsonPropertyName("count")]
        public int PeopleCount { get; set; }
        [JsonPropertyName("time")]
        public required string Time { get; set; }
        [JsonPropertyName("total_in")]
        public int TotalIn { get; set; }
        [JsonPropertyName("total_out")]
        public int TotalOut { get; set; }
    }

    public class CreateParkingRecordRequest
    {
        [JsonPropertyName("SiteId")]
        public required string DeviceSerial { get; set; }
        [JsonPropertyName("UsageCount")]
        public int ParkedNum { get; set; }
        [JsonPropertyName("ComputeTime")]
        public required string Time { get; set; }
        [JsonPropertyName("CarType")]
        public required string CarType { get; set; }
        [JsonPropertyName("RentCount")]
        public int RentCount { get; set; }
    }

    // Response DTOs for POST operations - matching old project format
    public class CreateFenceRecordResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "ok";
    }

    public class UpdateFenceHeartbeatResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "ok";
    }

    public class CreateCrowdRecordResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "ok";
    }

    public class CreateParkingRecordResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "ok";
    }

    // ── RtspToHttp 整合 DTOs ──

    /// <summary>
    /// GET /api/geofence/config 回傳的設備設定
    /// </summary>
    public class FenceDeviceConfigDto
    {
        [JsonPropertyName("serial")]
        public required string Serial { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("video_url")]
        public string? VideoUrl { get; set; }

        [JsonPropertyName("observing_time_start")]
        public string? ObservingTimeStart { get; set; }

        [JsonPropertyName("observing_time_end")]
        public string? ObservingTimeEnd { get; set; }

        [JsonPropertyName("zones")]
        public List<FenceZoneDto> Zones { get; set; } = new();

        [JsonPropertyName("camera_config")]
        public object? CameraConfig { get; set; }
    }

    /// <summary>
    /// 圍籬多邊形區域
    /// </summary>
    public class FenceZoneDto
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("Points")]
        public List<FencePointDto> Points { get; set; } = new();

        [JsonPropertyName("Cooldown")]
        public int Cooldown { get; set; } = 30;

        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("ZoneType")]
        public string ZoneType { get; set; } = "fence"; // "fence", "line", "area"
    }

    /// <summary>
    /// 圍籬座標點
    /// </summary>
    public class FencePointDto
    {
        [JsonPropertyName("X")]
        public double X { get; set; }

        [JsonPropertyName("Y")]
        public double Y { get; set; }
    }

    /// <summary>
    /// POST /api/geofence/zones 請求
    /// </summary>
    public class UpdateFenceZonesRequest
    {
        [JsonPropertyName("device_serial")]
        public required string DeviceSerial { get; set; }

        [JsonPropertyName("zones")]
        public List<FenceZoneDto> Zones { get; set; } = new();
    }

    /// <summary>
    /// POST /api/geofence/zones 回應
    /// </summary>
    public class UpdateFenceZonesResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "ok";

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    // ── RtspToHttp 外部告警 DTOs ──

    /// <summary>
    /// POST /api/external-alert/report 請求
    /// </summary>
    public class ExternalAlertDto
    {
        public string Source { get; set; } = "";
        public string AlertType { get; set; } = "";
        public string Status { get; set; } = "";
        public string Camera { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Snapshot { get; set; }
        public string Timestamp { get; set; } = "";
    }

    /// <summary>
    /// POST /api/external-alert/report 回應
    /// </summary>
    public class ExternalAlertResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "ok";
    }
}