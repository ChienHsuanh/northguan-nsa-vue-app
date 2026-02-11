namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    /// <summary>
    /// 快取服務接口
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 設置快取
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 獲取快取
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 檢查快取是否存在
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 刪除快取
        /// </summary>
        Task RemoveAsync(string key);
    }
}