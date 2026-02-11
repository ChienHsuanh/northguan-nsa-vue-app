// Report 相關的類型定義
import type { DateValue } from "@internationalized/date"

export interface Station {
  id: number
  name: string
  description?: string
}

export interface DateRange {
  start: DateValue
  end: DateValue
}

// 人流記錄 - 匹配後端 CrowdRecordListResponse
export interface CrowdRecord {
  id: number
  deviceSerial: string
  deviceName: string
  stationName: string
  area: number
  peopleCount: number
  density: number
  time: string
  timestamp: number
}

// 圍籬記錄 - 匹配後端 FenceRecordListResponse
export interface FenceRecord {
  id: number
  deviceSerial: string
  deviceName: string
  stationName: string
  event: string
  time: string
  timestamp: number
}

// 停車記錄 - 匹配後端 ParkingRecordListResponse
export interface ParkingRecord {
  id: number
  deviceSerial: string
  deviceName: string
  stationName: string
  totalSpaces: number
  parkedNum: number
  availableSpaces: number
  occupancyRate: number
  time: string
  timestamp: number
}

// 車流記錄 - 匹配後端 TrafficRecordListResponse
export interface TrafficRecord {
  id: number
  deviceSerial: string
  deviceName: string
  stationName: string
  city: string
  eTagNumber: string
  speedLimit: number
  vehicleCount: number
  averageSpeed: number
  speedStatus: string
  time: string
  timestamp: number
}

// 入場記錄
export interface AdmissionRecord {
  evtName: string
  ticketName: string
  date: string
  value: number
}

// 客群輪廓記錄 - E2TOURIST
export interface E2TouristRecord {
  ev_name: string
  yyyymmdd: string
  stay_mins: number
  male: number
  female: number
  age19: number
  age29: number
  age39: number
  age49: number
  age59: number
  age60: number
  travelerAreaPart1: TravelerAreaItem[]
  travelerAreaPart2: TravelerAreaItem[]
  travelerAreaPart3: TravelerAreaItem[]
  travelerAreaPart4: TravelerAreaItem[]
}

// 客群輪廓記錄 - G2TOURIST
export interface G2TouristRecord {
  name: string
  _time: string
  male: number
  female: number
  age0019: number
  age2029: number
  age3039: number
  age4049: number
  age5059: number
  age6099: number
  _national: [string, number][]
  travelerAreaPart1: TravelerAreaItem[]
  travelerAreaPart2: TravelerAreaItem[]
  travelerAreaPart3: TravelerAreaItem[]
  travelerAreaPart4: TravelerAreaItem[]
}

export interface TravelerAreaItem {
  label: string
  val: number
}

// API 響應類型
export interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
  totalCount?: number
}

export interface PaginatedResponse<T> extends ApiResponse<T[]> {
  totalCount: number
  currentPage: number
  pageSize: number
  totalPages: number
}

// 搜尋參數
export interface SearchParams {
  startDate: Date
  endDate: Date
  stationId?: string
  page?: number
  pageSize?: number
}

// 匯出參數
export interface ExportParams {
  startDate: Date
  endDate: Date
  stationId?: string
}