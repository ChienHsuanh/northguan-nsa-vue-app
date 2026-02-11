namespace northguan_nsa_vue_app.Server.DTOs
{
    // Enhanced Query Parameters for Records
    public class RecordQueryParameters
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public string Keyword { get; set; } = "";
        public string SortBy { get; set; } = "timestamp";
        public string SortOrder { get; set; } = "desc"; // asc or desc
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int>? StationIds { get; set; }
        public List<string>? DeviceSerials { get; set; }
    }

    // Enhanced Response with Pagination
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }

    // Record Service Response DTOs
    public class FenceRecordListResponse
    {
        public int Id { get; set; }
        public required string DeviceSerial { get; set; }
        public required string DeviceName { get; set; }
        public required string StationName { get; set; }
        public required string Event { get; set; }
        public string? ImageUrl { get; set; }
        public required string Time { get; set; }
        public long Timestamp { get; set; }
    }

    public class CrowdRecordListResponse
    {
        public int Id { get; set; }
        public required string DeviceSerial { get; set; }
        public required string DeviceName { get; set; }
        public required string StationName { get; set; }
        public int Area { get; set; }
        public int PeopleCount { get; set; }
        public double Density { get; set; }
        public required string Time { get; set; }
        public long Timestamp { get; set; }
    }

    public class ParkingRecordListResponse
    {
        public int Id { get; set; }
        public required string DeviceSerial { get; set; }
        public required string DeviceName { get; set; }
        public required string StationName { get; set; }
        public int TotalSpaces { get; set; }
        public int ParkedNum { get; set; }
        public int AvailableSpaces { get; set; }
        public double OccupancyRate { get; set; }
        public required string Time { get; set; }
        public long Timestamp { get; set; }
    }

    public class TrafficRecordListResponse
    {
        public int Id { get; set; }
        public required string DeviceSerial { get; set; }
        public required string DeviceName { get; set; }
        public required string StationName { get; set; }
        public required string City { get; set; }
        public required string ETagNumber { get; set; }
        public int SpeedLimit { get; set; }
        public int VehicleCount { get; set; }
        public decimal AverageSpeed { get; set; }
        public required string SpeedStatus { get; set; }
        public required string Time { get; set; }
        public long Timestamp { get; set; }
    }

    // Specific Query Parameters for each record type
    public class CrowdRecordQueryParameters : RecordQueryParameters
    {
        public int? MinPeopleCount { get; set; }
        public int? MaxPeopleCount { get; set; }
        public double? MinDensity { get; set; }
        public double? MaxDensity { get; set; }
        public int? MinArea { get; set; }
        public int? MaxArea { get; set; }
    }

    public class FenceRecordQueryParameters : RecordQueryParameters
    {
        public List<string>? EventTypes { get; set; }
    }

    public class ParkingRecordQueryParameters : RecordQueryParameters
    {
        public int? MinTotalSpaces { get; set; }
        public int? MaxTotalSpaces { get; set; }
        public double? MinOccupancyRate { get; set; }
        public double? MaxOccupancyRate { get; set; }
    }

    public class TrafficRecordQueryParameters : RecordQueryParameters
    {
        public int? MinVehicleCount { get; set; }
        public int? MaxVehicleCount { get; set; }
        public decimal? MinAverageSpeed { get; set; }
        public decimal? MaxAverageSpeed { get; set; }
        public List<string>? SpeedStatuses { get; set; }
        public List<string>? Cities { get; set; }
    }
}