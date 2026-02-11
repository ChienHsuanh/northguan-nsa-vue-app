using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProxyController> _logger;

        public ProxyController(IHttpClientFactory httpClientFactory, ILogger<ProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 代理人流設備的圖片 API 請求，解決 HTTPS 混合內容問題
        /// </summary>
        /// <param name="apiUrl">原始 API URL (base64 encoded)</param>
        /// <param name="channelid">通道 ID</param>
        /// <param name="i">時間戳</param>
        /// <returns>圖片內容</returns>
        [HttpGet("crowd-image")]
        public async Task<IActionResult> GetCrowdImage([FromQuery] string apiUrl, [FromQuery] string channelid = "cam1", [FromQuery] string? i = null)
        {
            try
            {
                // 解碼 base64 URL
                var decodedUrl = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(apiUrl));
                
                // 驗證 URL 是否包含 GetAIJpeg
                if (!decodedUrl.Contains("GetAIJpeg"))
                {
                    _logger.LogWarning("Invalid API URL provided: {Url}", decodedUrl);
                    return BadRequest("Invalid API URL");
                }

                // 構建完整的請求 URL
                var timestamp = i ?? new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
                var requestUrl = $"{decodedUrl}?channelid={channelid}&i={timestamp}";

                _logger.LogDebug("Proxying request to: {RequestUrl}", requestUrl);

                // 創建 HTTP 客戶端
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10); // 設置超時

                // 發送請求
                var response = await httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch image from {RequestUrl}. Status: {StatusCode}", 
                        requestUrl, response.StatusCode);
                    return StatusCode((int)response.StatusCode);
                }

                // 獲取圖片內容
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

                // 設置緩存頭
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                return File(imageBytes, contentType);
            }
            catch (FormatException)
            {
                _logger.LogWarning("Invalid base64 encoded API URL provided");
                return BadRequest("Invalid API URL encoding");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for crowd image proxy");
                return StatusCode(502, "Failed to fetch image from upstream server");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout for crowd image proxy");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in crowd image proxy");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// 測試代理端點是否正常工作
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}