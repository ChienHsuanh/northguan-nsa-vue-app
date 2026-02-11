import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import './assets/index.css'
import { useAuthStore } from './stores/auth'
import { appConfig, enforceHttpsRedirect } from './config/app.config'

console.log(import.meta.env)

// Enforce HTTPS redirect if configured
// enforceHttpsRedirect() // Disabled for HTTP-only deployment

// Log configuration in development
if (appConfig.development.enableDebugLogs) {
    console.log('🚀 Application starting with configuration:', appConfig)
    console.log('🔒 HTTPS Configuration:', {
        enabled: appConfig.https.enabled,
        port: appConfig.https.port,
        forceHttps: appConfig.https.forceHttps,
        currentProtocol: window.location.protocol
    })
}

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

// Provide global configuration
app.provide('appConfig', appConfig)

// 初始化認證狀態
const authStore = useAuthStore()
authStore.initializeAuth()

app.mount('#app')
