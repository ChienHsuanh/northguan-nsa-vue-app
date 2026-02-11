import { type AxiosRequestConfig } from 'axios'

// 請求緩存 Map，用於存儲正在進行的請求
const pendingRequests = new Map<string, AbortController>()

/**
 * 生成請求的唯一標識符
 * @param config axios 請求配置
 * @returns 請求的唯一標識符
 */
export function generateRequestKey(config: AxiosRequestConfig): string {
  const { method, url, params, data } = config
  return `${method?.toUpperCase()}_${url}_${JSON.stringify(params)}_${JSON.stringify(data)}`
}

/**
 * 添加請求到待處理列表
 * @param config axios 請求配置
 * @returns 是否成功添加（false 表示重複請求）
 */
export function addPendingRequest(config: AxiosRequestConfig): boolean {
  const requestKey = generateRequestKey(config)

  // 如果已經有相同的請求在進行中，取消新請求
  if (pendingRequests.has(requestKey)) {
    console.warn('重複請求被阻止:', requestKey)
    return false
  }

  // 創建 AbortController 用於取消請求
  const controller = new AbortController()
  config.signal = controller.signal

  // 添加到待處理列表
  pendingRequests.set(requestKey, controller)
  return true
}

/**
 * 從待處理列表中移除請求
 * @param config axios 請求配置
 */
export function removePendingRequest(config: AxiosRequestConfig): void {
  const requestKey = generateRequestKey(config)

  if (pendingRequests.has(requestKey)) {
    const controller = pendingRequests.get(requestKey)
    controller?.abort()
    pendingRequests.delete(requestKey)
  }
}

/**
 * 清除所有待處理的請求
 */
export function clearPendingRequests(): void {
  pendingRequests.forEach((controller) => {
    controller.abort()
  })
  pendingRequests.clear()
}

/**
 * 防抖配置選項
 */
export interface DebounceConfig {
  /** 是否啟用防抖，默認 true */
  enableDebounce?: boolean
  /** 防抖時間間隔（毫秒），默認 300ms */
  debounceTime?: number
  /** 是否顯示重複請求警告，默認 true */
  showWarning?: boolean
}

/**
 * 默認防抖配置
 */
export const defaultDebounceConfig: Required<DebounceConfig> = {
  enableDebounce: true,
  debounceTime: 300,
  showWarning: true
}