export enum ParkingStatus {
  Available = 0, // > 50%
  Limited = 1,   // 15% - 50%
  Full = 2,      // < 15%
}

export interface ParkingLot {
  id: string
  name: string
  address: string
  lat: number
  lng: number
  availableSpots: number
  totalSpots: number
  status: ParkingStatus
}

export enum AppView {
  Parking = 0,
  Traffic = 1,
}

export enum TrafficStatus {
  Smooth = 0,    // 順暢
  Slow = 1,      // 稍擠
  Congested = 2, // 壅塞
}

export interface TrafficSegment {
  id: string
  name: string
  address: string
  lat: number
  lng: number
  speed: number | null // null for "順暢"
  status: TrafficStatus
}