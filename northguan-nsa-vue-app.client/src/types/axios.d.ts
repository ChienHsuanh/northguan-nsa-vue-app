import { DebounceConfig } from '@/utils/requestDebounce'

declare module 'axios' {
  export interface AxiosRequestConfig {
    metadata?: {
      debounce?: Partial<DebounceConfig>
    }
  }
}