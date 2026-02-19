using northguan_nsa_vue_app.Server.Controllers;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string username, string password);
        Task<string> GeneratePasetoTokenAsync(ApplicationUser user);
        string GetCurrentUserId();
        string GetCurrentUserRole();
        bool IsAdmin();
        List<int> GetAvailableStationIds();
        bool HasStationPermission(int stationId);
        string HashPassword(ApplicationUser user, string password);
        bool VerifyPassword(ApplicationUser user, string password, string hashedPassword);

        Task<ApplicationUser> GetCurrentUserAsync();
        Task<ApplicationUser> CreateUserAsync(string email, string username, string name, string password, string role = "User");
        Task AddUserToStationAsync(string userId, int stationId);
        Task RemoveUserFromStationAsync(string userId, int stationId);
    }

    public interface IAccountService
    {
        Task<List<UserProfileResponse>> GetAccountListAsync(int page, int size, string keyword);
        Task<int> GetAccountCountAsync(string keyword);
        Task CreateAccountAsync(CreateAccountRequest request);

        Task UpdateAccountAsync(string id, UpdateAccountRequest request);
        Task DeleteAccountAsync(string id);
    }

    public interface IStationService
    {
        Task<List<Station>> GetStationsAsync(int page, int size, string keyword, List<int>? availableStationIds);
        Task<int> GetStationCountAsync(string keyword, List<int>? availableStationIds);
        Task<Station> GetStationByIdAsync(int id);
        Task CreateStationAsync(Station station);
        Task UpdateStationAsync(Station station);
        Task DeleteStationAsync(int id);
    }

    public interface IDeviceService
    {
        Task<List<DeviceListResponse>> GetDevicesAsync(string type, string keyword, int? page, int? size, List<int>? availableStationIds);
        Task<int> GetDeviceCountAsync(string type, string keyword, List<int>? availableStationIds);
        Task<DeviceListResponse?> GetDeviceByIdAsync(int deviceId);
        Task CreateDeviceAsync(CreateDeviceRequest request);
        Task UpdateDeviceAsync(int id, UpdateDeviceRequest request);
        Task DeleteDeviceAsync(int id, string type);
        Task<List<DeviceStatusResponse>> GetDevicesStatusAsync(int? page, int? size, string keyword, List<int> availableStationIds);
        Task<int> GetDevicesStatusCountAsync(List<int> availableStationIds);
        Task<List<DeviceStatusLogResponse>> GetDeviceStatusLogsAsync(int page, int size, string keyword, List<int> availableStationIds);
        Task<int> GetDeviceStatusLogCountAsync(string keyword, List<int> availableStationIds);

        // Device-specific methods
        Task<List<CrowdDevice>> GetCrowdDevicesByStationIdsAsync(List<int> stationIds);
        Task<List<ParkingDevice>> GetParkingDevicesByStationIdsAsync(List<int> stationIds);
        Task<List<TrafficDevice>> GetTrafficDevicesByStationIdsAsync(List<int> stationIds);
        Task<List<FenceDevice>> GetFenceDevicesByStationIdsAsync(List<int> stationIds);
        Task<List<HighResolutionDevice>> GetHighResolutionDevicesByStationIdsAsync(List<int> stationIds);
        Task<List<WaterDevice>> GetWaterDevicesByStationIdsAsync(List<int> stationIds);
    }

    public interface IFileService
    {
        string GetUploadFileUrl(string folder, string filename);
        Task<string> SaveUploadFileAsync(string folder, IFormFile file);
        Task<string> SaveBase64ImageAsync(string folder, string base64String, string? fileExtension = null);
        bool DeleteFile(string folder, string filename);
    }

    public interface IMapService
    {
        Task<LandmarksResponse> GetLandmarksAsync();
        Task<ParkingLandmarkResponse> GetLandmarkParkingAsync(int id);
        Task<TrafficLandmarkResponse> GetLandmarkTrafficAsync(int id);
        Task<CrowdLandmarkResponse> GetLandmarkCrowdAsync(int id);
        Task<FenceLandmarkResponse> GetLandmarkFenceAsync(int id);
        Task<HighResolutionLandmarkResponse> GetLandmarkHighResolutionAsync(int id);
        
        // 新增：取得壅塞/暢通狀態顯示的方法
        string GetTrafficCongestionDisplayStatus(decimal speed, int speedLimit);
        string GetCrowdCongestionDisplayStatus(double density);
    }

    // Overview Services
    public interface IParkingOverviewService
    {
        Task<ParkingConversionHistoryResponse> GetRecentConversionHistoryAsync(int stationId, int timeRange, List<int> availableStationIds);
        Task<ParkingRateResponse> GetRecentParkingRateAsync(int stationId, int timeRange, int limit, List<int> availableStationIds);
    }

    public interface ICrowdOverviewService
    {
        Task<CrowdCapacityHistoryResponse> GetRecentCapacityHistoryAsync(int stationId, int timeRange, List<int> availableStationIds);
        Task<CrowdCapacityRateResponse> GetRecentCapacityRateAsync(int stationId, int timeRange, int limit, List<int> availableStationIds);
    }

    public interface IFenceOverviewService
    {
        Task<FenceRecordHistoryResponse> GetRecentRecordHistoryAsync(int stationId, int timeRange, List<int> availableStationIds);
        Task<FenceRecentRecordDetailResponse> GetRecentRecordAsync(int stationId, int timeRange, int limit, List<int> availableStationIds);
        Task<FenceLatestRecordResponse> GetLatestRecordAsync(int stationId, List<int> availableStationIds);
    }

    public interface ITrafficOverviewService
    {
        Task<TrafficRoadConditionHistoryResponse> GetRecentRoadConditionHistoryAsync(int stationId, int timeRange, List<int> availableStationIds);
        Task<TrafficRoadConditionResponse> GetRecentRoadConditionAsync(int stationId, int timeRange, int limit, List<int> availableStationIds);
    }

    public interface IHighResolutionOverviewService
    {
        Task<HighResolutionOverviewResponse> GetOverviewInfoAsync(int stationId, List<int> availableStationIds);
    }

    // Record Services
    public interface IFenceRecordService
    {
        Task<PagedResponse<FenceRecordListResponse>> GetRecordsListAsync(FenceRecordQueryParameters parameters);
        Task<int> GetRecordsCountAsync(string keyword, List<int> availableStationIds);
        IAsyncEnumerable<FenceRecordListResponse> StreamAllFenceRecordsAsync(FenceRecordQueryParameters parameters);
    }

    public interface ICrowdRecordService
    {
        Task<PagedResponse<CrowdRecordListResponse>> GetRecordsListAsync(CrowdRecordQueryParameters parameters);
        Task<int> GetRecordsCountAsync(string keyword, List<int> availableStationIds);
        Task<byte[]> ExportRecordsAsync(string keyword, List<int> availableStationIds);
        IAsyncEnumerable<CrowdRecordListResponse> StreamAllCrowdRecordsAsync(CrowdRecordQueryParameters parameters);
    }

    public interface IParkingRecordService
    {
        Task<PagedResponse<ParkingRecordListResponse>> GetRecordsListAsync(ParkingRecordQueryParameters parameters);
        Task<int> GetRecordsCountAsync(string keyword, List<int> availableStationIds);
        IAsyncEnumerable<ParkingRecordListResponse> StreamAllParkingRecordsAsync(ParkingRecordQueryParameters parameters);
    }

    public interface ITrafficRecordService
    {
        Task<PagedResponse<TrafficRecordListResponse>> GetRecordsListAsync(TrafficRecordQueryParameters parameters);
        Task<int> GetRecordsCountAsync(string keyword, List<int> availableStationIds);
        IAsyncEnumerable<TrafficRecordListResponse> StreamAllTrafficRecordsAsync(TrafficRecordQueryParameters parameters);
    }

    public interface ISystemSettingService
    {
        Task<SystemSettingResponse> GetSystemSettingAsync();
        Task UpdateSystemSettingAsync(UpdateSystemSettingRequest request);
    }

    public interface IThirdPartyService
    {
        Task<CreateFenceRecordResponse> CreateFenceRecordAsync(CreateFenceRecordRequest request);
        Task<UpdateFenceHeartbeatResponse> UpdateFenceHeartbeatAsync(UpdateFenceHeartbeatRequest request);
        Task<CreateCrowdRecordResponse> CreateCrowdRecordAsync(CreateCrowdRecordRequest request);
        Task<CreateParkingRecordResponse> CreateParkingRecordAsync(CreateParkingRecordRequest request);
        Task<List<ThirdPartyStationInfo>> GetStationListAsync();
        Task<List<ThirdPartyFenceDeviceInfo>> GetFenceDeviceListAsync();
        Task<List<ThirdPartyCrowdDeviceInfo>> GetCrowdDeviceListAsync();
        Task<List<ThirdPartyParkingDeviceInfo>> GetParkingDeviceListAsync();
        Task<List<ThirdPartyTrafficDeviceInfo>> GetTrafficDeviceListAsync();
        Task<byte[]> CreateZeroTouchExcelAsync();
        Task<List<FenceDeviceConfigDto>> GetFenceDeviceConfigsAsync();
        Task<UpdateFenceZonesResponse> UpdateFenceZonesAsync(UpdateFenceZonesRequest request);
        Task<List<UnifiedDeviceConfigDto>> GetAllDeviceConfigsAsync();
    }
}