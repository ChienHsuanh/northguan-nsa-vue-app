namespace northguan_nsa_vue_app.Server.DTOs
{
    public class StationListRequest
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public string Keyword { get; set; } = "";
    }

    public class StationResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string? LineToken { get; set; }
        public bool EnableNotify { get; set; }
        public string CvpLocations { get; set; } = string.Empty;
        public List<DeviceResponse> CrowdDevices { get; set; } = new();
        public List<DeviceResponse> ParkingDevices { get; set; } = new();
        public List<DeviceResponse> TrafficDevices { get; set; } = new();
        public List<DeviceResponse> FenceDevices { get; set; } = new();
        public List<DeviceResponse> HighResolutionDevices { get; set; } = new();
    }

    public class DeviceResponse
    {
        public int Id { get; set; }
        public int StationID { get; set; }
        public required string Name { get; set; }
        public required string Serial { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string? VideoUrl { get; set; }
        public string? ApiUrl { get; set; }
        public int Area { get; set; }
        public int NumberOfParking { get; set; }
        public string? City { get; set; }
        public string? ETag_number { get; set; }
        public int SpeedLimit { get; set; }
        public string? ObservingTime { get; set; }
    }

    public class CreateStationRequest
    {
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string? LineToken { get; set; }
        public bool EnableNotify { get; set; } = false;
        public string? CvpLocations { get; set; }
    }

    public class UpdateStationRequest
    {
        public required string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string? LineToken { get; set; }
        public bool EnableNotify { get; set; } = false;
        public string? CvpLocations { get; set; }
    }

    public class CountResponse
    {
        public int Count { get; set; }
    }
}