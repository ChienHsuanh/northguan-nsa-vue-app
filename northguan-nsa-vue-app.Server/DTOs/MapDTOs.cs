namespace northguan_nsa_vue_app.Server.DTOs
{
    // Map Service Response DTOs
    public class LandmarksResponse
    {
        public List<LandmarkItem> Landmarks { get; set; } = new();
    }

    public class LandmarkItem
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public required string Type { get; set; }
        public required string Status { get; set; }
        public string? Serial { get; set; }
        public int StationID { get; set; }
        public required string StationName { get; set; }
        // 狀態和時間欄位
        public string? LatestOnlineTime { get; set; }
        public string? UpdatedAt { get; set; }
        // 設備特定屬性（根據需要）
        public int? Area { get; set; } // 人流設備
        public int? NumberOfParking { get; set; } // 停車場設備
        public int? SpeedLimit { get; set; } // 交通設備
        public string? VideoUrl { get; set; } // 人流和4K設備
        public string? ObservingTime { get; set; } // 圍籬設備
    }

    public class ParkingLandmarkResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public required string Serial { get; set; }
        public int NumberOfParking { get; set; }
        public required string Status { get; set; }
        public ParkingRecordInfo? LatestRecord { get; set; }
        public int CurrentParked { get; set; }
        public int AvailableSpaces { get; set; }
        public float OccupancyRate { get; set; }
        public Dictionary<string, double>? HistoryDatas { get; set; }
        public string? LastUpdateTime { get; set; }
    }

    public class ParkingRecordInfo
    {
        public long Time { get; set; }
        public int ParkedNum { get; set; }
        public string DeviceSerial { get; set; } = "";
    }

    public class TrafficLandmarkResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public required string Serial { get; set; }
        public required string City { get; set; }
        public required string ETagNumber { get; set; }
        public int SpeedLimit { get; set; }
        public required string Status { get; set; }
        public string? CongestionStatus { get; set; } // 新增：顯示壅塞/暢通狀態
        public TrafficRecordInfo? LatestRecord { get; set; }
        public int CurrentVehicleCount { get; set; }
        public decimal CurrentAverageSpeed { get; set; }
        public Dictionary<string, string>? HistoryDatas { get; set; }
        public string? LastUpdateTime { get; set; }
        public string? StationName { get; set; }
    }

    public class TrafficRecordInfo
    {
        public long Time { get; set; }
        public decimal SpaceMeanSpeed { get; set; }
        public int VehicleCount { get; set; }
        public string DeviceSerial { get; set; } = "";
    }

    public class CrowdLandmarkResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public required string Serial { get; set; }
        public int Area { get; set; }
        public required string VideoUrl { get; set; }
        public required string ApiUrl { get; set; }
        public required string Status { get; set; }
        public string? CongestionStatus { get; set; } // 新增：顯示擁擠/空曠狀態
        public CrowdRecordInfo? LatestRecord { get; set; }
        public int CurrentPeopleCount { get; set; }
        public double CrowdDensity { get; set; }
        public Dictionary<string, int>? HistoryDatas { get; set; }
        public string? LastUpdateTime { get; set; }
        public int TotalIn { get; set; }
    }

    public class CrowdRecordInfo
    {
        public long Time { get; set; }
        public int Count { get; set; }
        public int TotalIn { get; set; }
        public string DeviceSerial { get; set; } = "";
    }

    public class FenceLandmarkResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public required string Serial { get; set; }
        public required string ObservingTime { get; set; }
        public required string Status { get; set; }
        public FenceRecordInfo? LatestRecord { get; set; }
        public string? LastEvent { get; set; }
        public long? LastEventTime { get; set; }
        public Dictionary<string, int>? HistoryDatas { get; set; }
        public string? LastUpdateTime { get; set; }
        public string? StationName { get; set; }
        public List<FenceEventInfo>? Events { get; set; }
    }

    public class FenceRecordInfo
    {
        public long Time { get; set; }
        public int EventType { get; set; }
        public string? Photo { get; set; }
        public string DeviceSerial { get; set; } = "";
    }

    public class FenceEventInfo
    {
        public int Id { get; set; }
        public int EventType { get; set; }
        public string Time { get; set; } = "";
        public string Image { get; set; } = "";
    }

    public class HighResolutionLandmarkResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public required string Serial { get; set; }
        public required string VideoUrl { get; set; }
        public required string Status { get; set; }
        public string? LastUpdateTime { get; set; }
    }
}