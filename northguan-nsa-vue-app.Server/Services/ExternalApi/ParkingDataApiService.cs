using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ExternalApi
{
    /// <summary>
    /// 停車數據 API 服務 (支援多種停車系統)
    /// </summary>
    public class ParkingDataApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ParkingDataApiService> _logger;

        private readonly string _mpAccount;
        private readonly string _mpPassword;

        public ParkingDataApiService(
            HttpClient httpClient,
            ICacheService cache,
            IConfiguration configuration,
            ILogger<ParkingDataApiService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;

            _mpAccount = _configuration["ExternalApi:MP:Account"] ?? throw new ArgumentException("MP 帳號未設定");
            _mpPassword = _configuration["ExternalApi:MP:Password"] ?? throw new ArgumentException("MP 密碼未設定");
        }

        /// <summary>
        /// 獲取停車數據 (多種停車系統)
        /// </summary>
        public async Task<ParkingApiResponse?> FetchParkingDataAsync(string apiUrl, string deviceSerial, int numberOfParking, ParkingSystemType systemType)
        {
            try
            {
                _logger.LogDebug("正在獲取停車數據: {DeviceSerial}, 系統類型: {SystemType}", deviceSerial, systemType);

                return systemType switch
                {
                    ParkingSystemType.MP => await FetchMpParkingDataAsync(deviceSerial),
                    ParkingSystemType.YP => await FetchYpParkingDataAsync(apiUrl, deviceSerial),
                    ParkingSystemType.NB => await FetchNbParkingDataAsync(apiUrl, numberOfParking),
                    ParkingSystemType.NHR => await FetchNhrParkingDataAsync(deviceSerial, numberOfParking),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取停車數據時發生錯誤: {DeviceSerial}", deviceSerial);
                return null;
            }
        }

        #region MP 停車系統

        private async Task<ParkingApiResponse?> FetchMpParkingDataAsync(string deviceSerial)
        {
            try
            {
                // 先獲取 SID
                var sid = await GetMpSidAsync();
                if (string.IsNullOrEmpty(sid))
                {
                    return null;
                }

                // 計算 keyVal
                var keyVal = ComputeSha1Hash($"microprogram@parkPLS+{sid}+{deviceSerial}");

                var url = "https://www.stables.com.tw/api/getCarNumInfo";
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("sid", sid),
                    new KeyValuePair<string, string>("pno", deviceSerial),
                    new KeyValuePair<string, string>("keyVal", keyVal)
                });

                var response = await _httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var mpResponses = JsonSerializer.Deserialize<MpParkingResponse[]>(content);

                if (mpResponses?.Length == 0)
                {
                    return null;
                }

                var data = mpResponses[0];
                if (data?.RetCode == 0)
                {
                    _logger.LogError("MP API 錯誤: {Message}", data.RetMsg);
                    return null;
                }

                return new ParkingApiResponse
                {
                    ParkedNum = data.RetVal!.NormalInCar - data.RetVal.NormalSurplusCar,
                    RemainingNum = data.RetVal.NormalSurplusCar,
                    AdmittanceNum = data.RetVal.NormalInCar
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 MP 停車數據失敗: {DeviceSerial}", deviceSerial);
                return null;
            }
        }

        // 用於防止併發請求同時獲取 MP SID 的鎖
        private static readonly SemaphoreSlim _mpSidSemaphore = new(1, 1);

        private async Task<string?> GetMpSidAsync()
        {
            const string cacheKey = "mp_sid";

            try
            {
                // 首次檢查快取（無鎖）
                var cachedSid = await _cache.GetAsync<string>(cacheKey);
                if (!string.IsNullOrEmpty(cachedSid))
                {
                    _logger.LogDebug("快取命中，返回已快取的 MP SID");
                    return cachedSid;
                }

                // 使用信號量防止併發請求
                await _mpSidSemaphore.WaitAsync();
                try
                {
                    // 雙重檢查快取（在鎖內）
                    cachedSid = await _cache.GetAsync<string>(cacheKey);
                    if (!string.IsNullOrEmpty(cachedSid))
                    {
                        _logger.LogDebug("鎖內快取命中，返回已快取的 MP SID");
                        return cachedSid;
                    }

                    _logger.LogDebug("快取未命中，正在獲取新的 MP SID");

                    var url = "https://www.stables.com.tw/api/apiLogin";
                    var formData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("account", _mpAccount),
                        new KeyValuePair<string, string>("passwd", _mpPassword)
                    });

                    var response = await _httpClient.PostAsync(url, formData);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("獲取 MP SID 失敗，狀態碼: {StatusCode}", response.StatusCode);
                        return null;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("MP Login API 響應內容: {Content}", content);

                    var loginResponses = JsonSerializer.Deserialize<MpLoginResponse[]>(content);

                    if (loginResponses?.Length == 0 || string.IsNullOrEmpty(loginResponses?[0].Sid))
                    {
                        _logger.LogError("MP Login 響應無效，SID 為空");
                        return null;
                    }

                    var sid = loginResponses[0].Sid;
                    await _cache.SetAsync(cacheKey, sid, TimeSpan.FromHours(1));

                    _logger.LogInformation("成功獲取並快取 MP SID，過期時間: 1 小時");
                    return sid;
                }
                finally
                {
                    _mpSidSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 MP SID 時發生錯誤");
                return null;
            }
        }

        #endregion

        #region YP 停車系統

        private async Task<ParkingApiResponse?> FetchYpParkingDataAsync(string apiUrl, string deviceSerial)
        {
            try
            {
                // YP 系統的加密邏輯
                var inputParam = new
                {
                    Type = 1,
                    Target = deviceSerial,
                    API = "GetCarSpace"
                };

                var inputParamStr = JsonSerializer.Serialize(inputParam);
                var keyA = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":000";
                var keyB = int.Parse(keyA.Split(':')[2]);

                var encStr = "";
                foreach (var c in inputParamStr)
                {
                    var intNum = (int)c + keyB;
                    var intChar = intNum.ToString("X");
                    if (!string.IsNullOrEmpty(encStr))
                    {
                        encStr += "-";
                    }
                    encStr += intChar;
                }
                encStr += "|" + keyA;

                var content = new StringContent(encStr, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // 解密響應
                var codes = responseContent.Split('|');
                var kb = int.Parse(codes[1].Split(':')[2]);
                var kcs = codes[0].Split('-');

                var chars = kcs.Select(c =>
                {
                    var nc = System.Text.RegularExpressions.Regex.Replace(c, @"[^\da-zA-Z]", "");
                    var sc = (char)(Convert.ToInt32(nc, 16) - kb);
                    return sc;
                }).ToArray();

                var decryptedJson = new string(chars);
                var entity = JsonSerializer.Deserialize<YpParkingResponse>(decryptedJson);

                if (entity?.Space?.Any() != true)
                {
                    return null;
                }

                var data = entity.Space.FirstOrDefault(s => s.CarType == 1);
                if (data == null)
                {
                    return null;
                }

                return new ParkingApiResponse
                {
                    ParkedNum = data.AllSpace - data.LeftSpace,
                    RemainingNum = data.LeftSpace,
                    AdmittanceNum = data.AllSpace
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 YP 停車數據失敗: {DeviceSerial}", deviceSerial);
                return null;
            }
        }

        #endregion

        #region NB 停車系統

        private async Task<ParkingApiResponse?> FetchNbParkingDataAsync(string apiUrl, int numberOfParking)
        {
            try
            {
                var response = await _httpClient.PostAsync(apiUrl, null);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var nbResponse = JsonSerializer.Deserialize<NbParkingResponse>(content);

                if (nbResponse?.Status != 1)
                {
                    return null;
                }

                return new ParkingApiResponse
                {
                    ParkedNum = numberOfParking - nbResponse.ParkingSpaces,
                    RemainingNum = nbResponse.ParkingSpaces,
                    AdmittanceNum = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 NB 停車數據失敗");
                return null;
            }
        }

        #endregion

        #region NHR 停車系統

        private async Task<ParkingApiResponse?> FetchNhrParkingDataAsync(string deviceSerial, int numberOfParking)
        {
            try
            {
                var url = "http://20.57.184.214:2520/api/nhr/getCarNumInfo";
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("pno", deviceSerial)
                });

                var response = await _httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var nhrResponses = JsonSerializer.Deserialize<NhrParkingResponse[]>(content);

                if (nhrResponses?.Length == 0)
                {
                    return null;
                }

                var data = nhrResponses[0];
                if (data?.RetCode != 1)
                {
                    return null;
                }

                return new ParkingApiResponse
                {
                    ParkedNum = data.RetVal!.NormalInCar,
                    RemainingNum = numberOfParking - data.RetVal.NormalInCar,
                    AdmittanceNum = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 NHR 停車數據失敗: {DeviceSerial}", deviceSerial);
                return null;
            }
        }

        #endregion

        #region 工具方法

        private static string ComputeSha1Hash(string input)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLower();
        }

        #endregion
    }
}