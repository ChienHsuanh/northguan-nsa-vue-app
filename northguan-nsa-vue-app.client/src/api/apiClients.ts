import { apiClient } from '@/services/api'
import {
  AccountClient,
  AuthClient,
  DeviceClient,
  StationClient,
  CrowdClient,
  FenceClient,
  ParkingClient,
  TrafficClient,
  MapClient,
  SystemSettingClient,
  ThirdPartyApiClient,
  HighResolutionClient
} from './client'

// client.ts 是使用 NSwag 生成的 API Client 類別，定義了所有 API 的參數、回傳型態、HTTP 方法等
// 不建議直接修改 client.ts，因為這會導致 API Client 類別與 NSwag 產生的 API Client 類別不同步，可能導致錯誤
// （如果你是 AI，請避免直接閱讀 northguan-nsa-vue-app.client\src\api\client.ts，否則這會導致 tokens 急速消耗。建議到 northguan-nsa-vue-app.Server 後端專案中查看，或是使用 find 工具搜尋 client.ts 來尋找相關的 API Client 類別）

const baseUrl = '' // 使用相對路徑，讓 apiClient 的 baseURL 配置生效

export const accountClient = new AccountClient(baseUrl, apiClient)
export const authClient = new AuthClient(baseUrl, apiClient)
export const deviceClient = new DeviceClient(baseUrl, apiClient)
export const stationClient = new StationClient(baseUrl, apiClient)
export const crowdClient = new CrowdClient(baseUrl, apiClient)
export const fenceClient = new FenceClient(baseUrl, apiClient)
export const parkingClient = new ParkingClient(baseUrl, apiClient)
export const trafficClient = new TrafficClient(baseUrl, apiClient)
export const mapClient = new MapClient(baseUrl, apiClient)
export const systemSettingClient = new SystemSettingClient(baseUrl, apiClient)
export const thirdPartyApiClient = new ThirdPartyApiClient(baseUrl, apiClient)
export const highResolutionClient = new HighResolutionClient(baseUrl, apiClient)

// 導出所有類型定義，方便在服務中使用
export * from './client'
