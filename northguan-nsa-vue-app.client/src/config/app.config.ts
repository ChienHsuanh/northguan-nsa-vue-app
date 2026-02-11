// Application Configuration
export interface AppConfig {
  api: {
    baseUrl: string;
    timeout: number;
  };
  https: {
    enabled: boolean;
    port: number;
    forceHttps: boolean;
  };
  development: {
    enableMockData: boolean;
    enableDebugLogs: boolean;
  };
}

// Environment-based configuration
const isDevelopment = import.meta.env.DEV;
const isProduction = import.meta.env.PROD;

// Get HTTPS configuration from environment variables or defaults
const getHttpsConfig = () => {
  // Check for environment variables (can be set by build process)
  const httpsEnabled = import.meta.env.VITE_HTTPS_ENABLED === 'true';
  const httpsPort = parseInt(import.meta.env.VITE_HTTPS_PORT || '443');
  const forceHttps = import.meta.env.VITE_FORCE_HTTPS === 'true';

  // Default configurations based on environment
  if (isDevelopment) {
    return {
      enabled: httpsEnabled || false, // Default to false in development
      port: httpsPort || 5114, // Use HTTP port
      forceHttps: forceHttps || false // Don't force in development
    };
  } else {
    return {
      enabled: httpsEnabled || false, // Default to false in production
      port: httpsPort || 80, // Use HTTP port
      forceHttps: forceHttps || false // Don't force in production
    };
  }
};

// Get API base URL based on HTTPS configuration
const getApiBaseUrl = () => {
  const httpsConfig = getHttpsConfig();

  // In development, use the dev server proxy
  if (isDevelopment) {
    return ''; // Use relative URLs, proxy will handle
  }

  // In production, construct the full URL
  const protocol = httpsConfig.enabled ? 'https' : 'http';
  const port = httpsConfig.enabled ?
    (httpsConfig.port === 443 ? '' : `:${httpsConfig.port}`) :
    (httpsConfig.port === 80 ? '' : `:${httpsConfig.port}`);

  const host = import.meta.env.VITE_API_HOST || window.location.hostname;

  return `${protocol}://${host}${port}`;
};

// Main configuration object
export const appConfig: AppConfig = {
  api: {
    baseUrl: getApiBaseUrl(),
    timeout: parseInt(import.meta.env.VITE_API_TIMEOUT || '30000')
  },
  https: getHttpsConfig(),
  development: {
    enableMockData: import.meta.env.VITE_ENABLE_MOCK_DATA === 'true',
    enableDebugLogs: isDevelopment || import.meta.env.VITE_ENABLE_DEBUG_LOGS === 'true'
  }
};

// Helper functions
export const getApiUrl = (endpoint: string): string => {
  const baseUrl = appConfig.api.baseUrl;
  const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;

  if (baseUrl) {
    return `${baseUrl}/api${cleanEndpoint}`;
  } else {
    return `/api${cleanEndpoint}`;
  }
};

export const shouldForceHttps = (): boolean => {
  return appConfig.https.enabled && appConfig.https.forceHttps;
};

export const getCurrentProtocol = (): string => {
  if (shouldForceHttps()) {
    return 'https:';
  }
  return appConfig.https.enabled ? 'https:' : 'http:';
};

// Redirect to HTTPS if needed
export const enforceHttpsRedirect = (): void => {
  if (shouldForceHttps() && window.location.protocol === 'http:') {
    const httpsUrl = window.location.href.replace('http:', 'https:');
    window.location.replace(httpsUrl);
  }
};

// Debug logging
if (appConfig.development.enableDebugLogs) {
  console.log('App Configuration:', appConfig);
}
