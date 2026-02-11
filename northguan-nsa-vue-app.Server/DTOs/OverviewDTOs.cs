namespace northguan_nsa_vue_app.Server.DTOs
{
    // Overview Service Response DTOs

    // Parking Overview DTOs
    public class ParkingConversionHistoryResponse
    {
        public List<ParkingHistoryData> Data { get; set; } = new List<ParkingHistoryData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class ParkingHistoryData
    {
        public required string Time { get; set; }
        public long Timestamp { get; set; }
        public int TotalSpaces { get; set; }
        public int ParkedNum { get; set; }
        public int AvailableSpaces { get; set; }
        public double OccupancyRate { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
    }

    public class ParkingRateResponse
    {
        public List<ParkingRateData> Data { get; set; } = new List<ParkingRateData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class ParkingRateData
    {
        public int DeviceId { get; set; }
        public int StationId { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
        public double AverageOccupancyRate { get; set; }
        public int TotalRecords { get; set; }
        public required string LatestTime { get; set; }
        public string Status { get; set; } = "online";
        public double Rate { get; set; }
    }

    // Crowd Overview DTOs
    public class CrowdCapacityHistoryResponse
    {
        public List<CrowdHistoryData> Data { get; set; } = new List<CrowdHistoryData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class CrowdHistoryData
    {
        public required string Time { get; set; }
        public long Timestamp { get; set; }
        public int PeopleCount { get; set; }
        public int Area { get; set; }
        public double Density { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
    }

    public class CrowdCapacityRateResponse
    {
        public List<CrowdRateData> Data { get; set; } = new List<CrowdRateData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class CrowdRateData
    {
        public int DeviceId { get; set; }
        public int StationId { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
        public double AverageDensity { get; set; }
        public int AveragePeopleCount { get; set; }
        public int TotalRecords { get; set; }
        public required string LatestTime { get; set; }
        public string Status { get; set; } = "online";
        public double Rate { get; set; }
    }

    // Fence Overview DTOs
    public class FenceRecordHistoryResponse
    {
        public List<FenceHistoryData> Data { get; set; } = new List<FenceHistoryData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class FenceHistoryData
    {
        public required string Time { get; set; }
        public long Timestamp { get; set; }
        public int EventType { get; set; }
        public required string Event { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
    }


    // New DTO that matches the PHP response structure
    public class FenceRecentRecordDetailResponse
    {
        public List<FenceRecentRecordDetail> Data { get; set; } = new List<FenceRecentRecordDetail>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class FenceRecentRecordDetail
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public required string DeviceName { get; set; }
        public required string Station { get; set; }
        public int EventType { get; set; }
        public required string Event { get; set; }
        public required string Title { get; set; }
        public string ImageUrl { get; set; } = "";
        public required string Time { get; set; }
        public long Timestamp { get; set; }
    }

    public class FenceLatestRecordResponse
    {
        public List<FenceLatestData> Data { get; set; } = new List<FenceLatestData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class FenceLatestData
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
        public int EventType { get; set; }
        public required string Event { get; set; }
        public string ImageUrl { get; set; } = "";
        public required string Time { get; set; }
        public long Timestamp { get; set; }
    }

    // Traffic Overview DTOs
    public class TrafficRoadConditionHistoryResponse
    {
        public List<TrafficHistoryData> Data { get; set; } = new List<TrafficHistoryData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class TrafficHistoryData
    {
        public required string Time { get; set; }
        public long Timestamp { get; set; }
        public int VehicleCount { get; set; }
        public decimal AverageSpeed { get; set; }
        public required string SpeedStatus { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
    }

    public class TrafficRoadConditionResponse
    {
        public List<TrafficConditionData> Data { get; set; } = new List<TrafficConditionData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class TrafficConditionData
    {
        public int DeviceId { get; set; }
        public int StationId { get; set; }
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
        public decimal AverageSpeed { get; set; }
        public int AverageVehicleCount { get; set; }
        public int SpeedLimit { get; set; }
        public required string SpeedStatus { get; set; }
        public int TotalRecords { get; set; }
        public required string LatestTime { get; set; }
        public string Status { get; set; } = "online";
        public double Rate { get; set; }
    }

    // High Resolution Overview DTOs
    public class HighResolutionOverviewResponse
    {
        public List<HighResolutionData> Data { get; set; } = new List<HighResolutionData>();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    public class HighResolutionData
    {
        public required string StationName { get; set; }
        public required string DeviceName { get; set; }
        public required string VideoUrl { get; set; }
        public required string Status { get; set; }
        public required string LatestOnlineTime { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}