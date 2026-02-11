/**
 * 人流擁擠情況計算工具
 * 根據平均空間密度判斷人流狀況
 */

export interface CrowdStatus {
  level: "smooth" | "moderate" | "congested";
  text: string;
  color: string;
  bgColor: string;
  description: string;
}

/**
 * 根據人流密度計算擁擠情況
 * @param density 人流密度 (人/平方公尺)
 * @returns 擁擠情況對象
 */
export function getCrowdStatus(density: number): CrowdStatus {
  if (density > 1.2) {
    return {
      level: "congested",
      text: "人流壅塞",
      color: "text-red-600",
      bgColor: "bg-red-100",
      description: "平均空間>1.2人/平方公尺",
    };
  } else if (density >= 0.7 && density <= 1.2) {
    return {
      level: "moderate",
      text: "人流稍擠",
      color: "text-yellow-600",
      bgColor: "bg-yellow-100",
      description: "平均空間介於0.7~1.2人/平方公尺",
    };
  } else {
    return {
      level: "smooth",
      text: "人流順暢",
      color: "text-green-600",
      bgColor: "bg-green-100",
      description: "平均空間<0.7人/平方公尺",
    };
  }
}

/**
 * 獲取擁擠情況的圖標類別
 * @param level 擁擠等級
 * @returns CSS 類別字串
 */
export function getCrowdStatusIconClass(level: CrowdStatus["level"]): string {
  switch (level) {
    case "smooth":
      return "text-green-500";
    case "moderate":
      return "text-yellow-500";
    case "congested":
      return "text-red-500";
    default:
      return "text-gray-500";
  }
}

/**
 * 獲取擁擠情況的指示器顏色
 * @param level 擁擠等級
 * @returns CSS 背景顏色類別
 */
export function getCrowdStatusIndicatorClass(level: CrowdStatus["level"]): string {
  switch (level) {
    case "smooth":
      return "bg-green-500";
    case "moderate":
      return "bg-yellow-500";
    case "congested":
      return "bg-red-500";
    default:
      return "bg-gray-500";
  }
}

/**
 * 根據密度獲取狀態文字 (用於 CrowdReport)
 * @param density 密度值
 * @returns 狀態文字
 */
export function getCrowdStatusText(density: number): string {
  if (density >= 1.2) return "擁擠";
  if (density >= 0.7) return "稍擠";
  return "正常";
}

/**
 * 根據狀態文字獲取對應的圖標路徑 (用於 CrowdReport)
 * @param status 狀態文字
 * @returns 圖標路徑
 */
export function getCrowdStatusIcon(status: string): string {
  switch (status) {
    case "擁擠":
      return "/images/icons/crowd-red.svg";
    case "稍擠":
      return "/images/icons/crowd-yellow.svg";
    case "正常":
    default:
      return "/images/icons/crowd-green.svg";
  }
}

/**
 * 根據密度獲取狀態文字 (用於 CrowdOverview，不同的閾值)
 * @param density 密度值
 * @returns 狀態文字
 */
export function getDensityStatus(density: number): string {
  if (density >= 10) return "擁擠";
  if (density >= 5) return "稍擠";
  return "正常";
}

/**
 * 根據密度獲取狀態顏色類別 (用於 CrowdOverview)
 * @param density 密度值
 * @returns CSS 背景顏色類別
 */
export function getDensityStatusColor(density: number): string {
  if (density >= 10) return "bg-red-500";
  if (density >= 5) return "bg-yellow-500";
  return "bg-green-500";
}
