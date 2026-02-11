# ExternalApi 服務說明

## 📁 服務架構

### IExternalApiService
主要外部 API 服務接口，提供統一的 API 調用方法。

### ExternalApiService
外部 API 服務實作，支援多種第三方系統：

#### 人流數據 (A3DPC)
- HTTP Digest 認證
- 即時人流統計數據

#### 停車數據 (多系統支援)
- **MP (MicroProgram)**: SHA1 加密 + SID 認證
- **YP (YouParking)**: 自定義加密算法
- **NB (Nobel)**: 簡單 POST 請求
- **NHR**: 表單數據提交

#### 交通數據 (TDX)
- OAuth2 Bearer Token 認證
- ETag 交通流量數據

### ParkingSystemDetector
自動檢測停車系統類型的工具類。

## 🔧 使用方式

```csharp
// 注入服務
private readonly IExternalApiService _externalApi;

// 調用 API
var crowdData = await _externalApi.FetchCrowdDataAsync(apiUrl);
var parkingData = await _externalApi.FetchParkingDataAsync(apiUrl, deviceSerial, systemType);
var trafficData = await _externalApi.FetchTrafficDataAsync(eTagNumber, city);
```

## ⚙️ 配置

在 `appsettings.ExternalApi.json` 中配置各系統的 API 憑證和端點。

## 🔐 安全性

- 所有敏感憑證都通過配置管理
- 支援各種認證機制
- 自動 Token 刷新和快取