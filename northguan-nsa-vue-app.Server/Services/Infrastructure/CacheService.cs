using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();

                if (expiry.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiry.Value;
                }
                else
                {
                    // 預設過期時間為 1 小時
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                }

                // 設置快取優先級 - 對於 token 類型的快取使用高優先級
                if (key.Contains("token", StringComparison.OrdinalIgnoreCase))
                {
                    options.Priority = CacheItemPriority.High;
                }
                else
                {
                    options.Priority = CacheItemPriority.Normal;
                }

                // 自動計算快取項目的大小
                // 計算 JSON 字串的位元組大小作為快取項目的大小，使用 UTF-8 編碼來取得精確的位元組數
                var jsonValue = JsonSerializer.Serialize(value);
                int sizeInBytes = Encoding.UTF8.GetByteCount(jsonValue);
                options.SetSize(sizeInBytes);

                // 添加過期回調以便追蹤
                options.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogDebug("快取項目被移除，Key: {Key}, 原因: {Reason}", key, reason);
                });

                _memoryCache.Set(key, value, options);

                var absoluteExpiry = DateTime.Now.Add(options.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromHours(1));
                _logger.LogInformation("快取設置成功，Key: {Key}, 大小: {Size} bytes, 過期時間: {Expiry}, 絕對過期時間: {AbsoluteExpiry}",
                    key, sizeInBytes, expiry, absoluteExpiry.ToString("HH:mm:ss"));

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設置快取時發生錯誤，Key: {Key}", key);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    if (value is T typedValue)
                    {
                        _logger.LogDebug("快取命中，Key: {Key}", key);
                        return typedValue;
                    }
                    else
                    {
                        _logger.LogWarning("快取值類型不匹配，Key: {Key}, 期望類型: {ExpectedType}, 實際類型: {ActualType}",
                            key, typeof(T).Name, value?.GetType().Name ?? "null");
                    }
                }
                else
                {
                    _logger.LogDebug("快取未命中，Key: {Key}", key);
                }

                await Task.CompletedTask;
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取快取時發生錯誤，Key: {Key}", key);
                return default;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var exists = _memoryCache.TryGetValue(key, out _);
                _logger.LogDebug("檢查快取存在性，Key: {Key}, 存在: {Exists}", key, exists);

                await Task.CompletedTask;
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查快取存在性時發生錯誤，Key: {Key}", key);
                return false;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("快取移除成功，Key: {Key}", key);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除快取時發生錯誤，Key: {Key}", key);
                throw;
            }
        }
    }
}