using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    /// <summary>
    /// 設備客戶端服務接口
    /// </summary>
    public interface IDeviceClientService
    {
        /// <summary>
        /// 獲取人流數據
        /// </summary>
        Task<CrowdDeviceDataResponse?> FetchCrowdDataAsync(string apiUrl);

        /// <summary>
        /// 獲取停車數據
        /// </summary>
        Task<ParkingDeviceDataResponse?> FetchParkingDataAsync(string apiUrl);

        /// <summary>
        /// 獲取交通數據
        /// </summary>
        Task<TrafficDeviceDataResponse?> FetchTrafficDataAsync(string apiUrl);

        /// <summary>
        /// 獲取 CVP 數據
        /// </summary>
        Task<CvpDeviceDataResponse?> FetchCvpDataAsync(string apiUrl);
    }
}