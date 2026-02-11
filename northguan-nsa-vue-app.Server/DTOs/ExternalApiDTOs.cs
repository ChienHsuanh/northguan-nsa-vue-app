using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace northguan_nsa_vue_app.Server.DTOs
{
    /// <summary>
    /// 人流 API 響應
    /// </summary>
    public class CrowdApiResponse
    {
        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("total_in")]
        public int TotalIn { get; set; }

        [JsonPropertyName("total_out")]
        public int TotalOut { get; set; }

        [JsonPropertyName("occupancy")]
        public int Occupancy { get; set; }
    }

    /// <summary>
    /// 停車 API 響應
    /// </summary>
    public class ParkingApiResponse
    {
        public int ParkedNum { get; set; }
        public int RemainingNum { get; set; }
        public int AdmittanceNum { get; set; }
        public double ConvertRate { get; set; }
    }

    /// <summary>
    /// 交通 API 響應
    /// </summary>
    public class TrafficApiResponse
    {
        public int TravelTime { get; set; }
        public double SpaceMeanSpeed { get; set; }
        public DateTime DataCollectTime { get; set; }
        public int VehicleCount { get; set; }
    }

    /// <summary>
    /// TDX Token 響應
    /// </summary>
    public class TdxTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    /// <summary>
    /// TDX ETag 數據響應
    /// </summary>
    public class TdxETagResponse
    {
        [JsonPropertyName("UpdateTime")]
        public string? UpdateTime { get; set; }

        [JsonPropertyName("UpdateInterval")]
        public int UpdateInterval { get; set; }

        [JsonPropertyName("SrcUpdateTime")]
        public string? SrcUpdateTime { get; set; }

        [JsonPropertyName("SrcUpdateInterval")]
        public int SrcUpdateInterval { get; set; }

        [JsonPropertyName("AuthorityCode")]
        public string? AuthorityCode { get; set; }

        [JsonPropertyName("LinkVersion")]
        public string? LinkVersion { get; set; }

        [JsonPropertyName("ETagPairLives")]
        public List<ETagPairLive> ETagPairLives { get; set; } = new();

        [JsonPropertyName("Count")]
        public int Count { get; set; }
    }

    public class ETagPairLive
    {
        [JsonPropertyName("ETagPairID")]
        public string? ETagPairID { get; set; }

        [JsonPropertyName("SubAuthorityCode")]
        public string? SubAuthorityCode { get; set; }

        [JsonPropertyName("StartETagStatus")]
        public int StartETagStatus { get; set; }

        [JsonPropertyName("EndETagStatus")]
        public int EndETagStatus { get; set; }

        [JsonPropertyName("DataCollectTime")]
        public string? DataCollectTime { get; set; }

        [JsonPropertyName("StartTime")]
        public string? StartTime { get; set; }

        [JsonPropertyName("EndTime")]
        public string? EndTime { get; set; }

        [JsonPropertyName("Flows")]
        public List<TrafficFlow> Flows { get; set; } = new();
    }

    public class TrafficFlow
    {
        [JsonPropertyName("VehicleType")]
        public int VehicleType { get; set; }

        [JsonPropertyName("TravelTime")]
        public int TravelTime { get; set; }

        [JsonPropertyName("SpaceMeanSpeed")]
        public double SpaceMeanSpeed { get; set; }
        [JsonPropertyName("VehicleCount")]
        public int VehicleCount { get; set; }
    }

    /// <summary>
    /// MP 停車系統響應
    /// </summary>
    public class MpParkingResponse
    {
        [JsonPropertyName("retCode")]
        public int RetCode { get; set; }

        [JsonPropertyName("retMsg")]
        public string? RetMsg { get; set; }

        [JsonPropertyName("retVal")]
        public MpParkingData? RetVal { get; set; }
    }

    public class MpParkingData
    {
        [JsonPropertyName("normalInCar")]
        public int NormalInCar { get; set; }

        [JsonPropertyName("normalSurplusCar")]
        public int NormalSurplusCar { get; set; }
    }

    /// <summary>
    /// MP 登入響應
    /// </summary>
    public class MpLoginResponse
    {
        [JsonPropertyName("sid")]
        public string? Sid { get; set; }
    }

    /// <summary>
    /// YP 停車系統響應
    /// </summary>
    public class YpParkingResponse
    {
        [JsonPropertyName("Space")]
        public List<YpSpaceData> Space { get; set; } = new();
    }

    public class YpSpaceData
    {
        [JsonPropertyName("CarType")]
        public int CarType { get; set; }

        [JsonPropertyName("AllSpace")]
        public int AllSpace { get; set; }

        [JsonPropertyName("LeftSpace")]
        public int LeftSpace { get; set; }
    }

    /// <summary>
    /// NB 停車系統響應
    /// </summary>
    public class NbParkingResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("parkingspaces")]
        public int ParkingSpaces { get; set; }
    }

    /// <summary>
    /// NHR 停車系統響應
    /// </summary>
    public class NhrParkingResponse
    {
        [JsonPropertyName("retCode")]
        public int RetCode { get; set; }

        [JsonPropertyName("retVal")]
        public NhrParkingData? RetVal { get; set; }
    }

    public class NhrParkingData
    {
        [JsonPropertyName("normalInCar")]
        public int NormalInCar { get; set; }
    }

    /// <summary>
    /// 交通部數據上傳請求
    /// </summary>
    public class TransportationUploadRequest
    {
        public string DataType { get; set; } = string.Empty; // "crowd" or "parking"
        public string DeviceSerial { get; set; } = string.Empty;
        public object Data { get; set; } = new();
    }

    /// <summary>
    /// 人流數據上傳格式
    /// </summary>
    public class CrowdUploadData
    {
        public string Serial { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// 停車數據上傳格式
    /// </summary>
    public class ParkingUploadData
    {
        public string Serial { get; set; } = string.Empty;
        public int RemainingNum { get; set; }
        public int AdmittanceNum { get; set; }
    }
}