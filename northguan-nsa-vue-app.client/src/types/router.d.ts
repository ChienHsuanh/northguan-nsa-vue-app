// 擴展 Vue Router 的 meta 類型定義
import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    requiresAdmin?: boolean
    title?: string
  }
}