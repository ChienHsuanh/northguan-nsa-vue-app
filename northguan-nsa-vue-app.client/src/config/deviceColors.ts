// 設備類型顏色配置 - 統一顏色常量
export const DEVICE_COLORS = {
  parking: {
    primary: '#7C3AED',      // violet-600
    primaryDark: '#6D28D9',  // violet-700
    light: '#F3F4F6',        // violet-50 background
    text: '#7C3AED',         // violet-600 text
    textLight: '#8B5CF6',    // violet-500 text
    bg: 'bg-violet-600',
    bgLight: 'bg-violet-50',
    textClass: 'text-violet-600',
    textLightClass: 'text-violet-500',
    border: 'border-violet-500'
  },
  traffic: {
    primary: '#1E40AF',      // blue-700  
    primaryDark: '#1E3A8A',  // blue-800
    light: '#EFF6FF',        // blue-50 background
    text: '#1E40AF',         // blue-700 text
    textLight: '#3B82F6',    // blue-500 text
    bg: 'bg-blue-700',
    bgLight: 'bg-blue-50',
    textClass: 'text-blue-700',
    textLightClass: 'text-blue-500',
    border: 'border-blue-700'
  },
  crowd: {
    primary: '#0F766E',      // teal-700
    primaryDark: '#134E4A',  // teal-800
    light: '#F0FDFA',        // teal-50 background
    text: '#0F766E',         // teal-700 text
    textLight: '#14B8A6',    // teal-500 text
    bg: 'bg-teal-700',
    bgLight: 'bg-teal-50',
    textClass: 'text-teal-700',
    textLightClass: 'text-teal-500',
    border: 'border-teal-700'
  },
  fence: {
    primary: '#4338CA',      // indigo-700
    primaryDark: '#3730A3',  // indigo-800
    light: '#EEF2FF',        // indigo-50 background
    text: '#4338CA',         // indigo-700 text
    textLight: '#6366F1',    // indigo-500 text
    bg: 'bg-indigo-700',
    bgLight: 'bg-indigo-50',
    textClass: 'text-indigo-700',
    textLightClass: 'text-indigo-500',
    border: 'border-indigo-700'
  },
  highResolution: {
    primary: '#475569',      // slate-600
    primaryDark: '#334155',  // slate-700
    light: '#F8FAFC',        // slate-50 background
    text: '#475569',         // slate-600 text
    textLight: '#64748B',    // slate-500 text
    bg: 'bg-slate-600',
    bgLight: 'bg-slate-50',
    textClass: 'text-slate-600',
    textLightClass: 'text-slate-500',
    border: 'border-slate-600'
  }
} as const

export type DeviceType = keyof typeof DEVICE_COLORS
export type DeviceColors = typeof DEVICE_COLORS[DeviceType]

// 獲取設備類型顏色的輔助函數
export const getDeviceColors = (deviceType: DeviceType) => {
  return DEVICE_COLORS[deviceType]
}

// 獲取設備類型的主要顏色
export const getDevicePrimaryColor = (deviceType: DeviceType) => {
  return DEVICE_COLORS[deviceType].primary
}

// 設備類型映射
export const DEVICE_TYPE_MAP = {
  'parking': 'parking',
  'traffic': 'traffic', 
  'crowd': 'crowd',
  'fence': 'fence',
  'highResolution': 'highResolution'
} as const