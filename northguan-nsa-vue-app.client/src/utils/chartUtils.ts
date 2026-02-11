/**
 * 圖表相關工具函數
 * 統一管理圖表的顏色、格式化等功能
 */

/**
 * 圖表顏色調色板
 */
export const CHART_COLORS = [
  '#3b82f6',
  '#ef4444', 
  '#10b981',
  '#f59e0b',
  '#8b5cf6',
  '#06b6d4',
  '#84cc16',
  '#f97316',
  '#ec4899',
  '#6366f1',
]

/**
 * 根據索引獲取圖表顏色
 * @param index 索引值
 * @returns 顏色字串
 */
export function getChartColor(index: number): string {
  return CHART_COLORS[index % CHART_COLORS.length]
}

/**
 * 格式化容量圖表工具提示
 * @param params ECharts 參數
 * @returns 格式化的工具提示 HTML
 */
export function formatCapacityTooltip(params: any): string {
  if (!params || params.length === 0) return ''

  const time = params[0].axisValue
  let tooltip = `<div><strong>${time}</strong></div>`

  // Group by device
  const deviceData: Record<string, { density?: number; peopleCount?: number }> = {}
  params.forEach((param: any) => {
    if (param.value !== null) {
      const deviceName = param.seriesName.replace(/ \((密度|人數)\)$/, '')
      if (!deviceData[deviceName]) deviceData[deviceName] = {}

      if (param.seriesName.includes('(密度)')) {
        deviceData[deviceName].density = param.value
      } else if (param.seriesName.includes('(人數)')) {
        deviceData[deviceName].peopleCount = param.value
      }
    }
  })

  Object.entries(deviceData).forEach(([deviceName, values]) => {
    tooltip += `<div style="margin: 4px 0;">`
    tooltip += `<span style="color: ${params.find((p: any) => p.seriesName.startsWith(deviceName))?.color};">●</span> `
    tooltip += `<strong>${deviceName}</strong><br/>`
    if (values.density !== undefined) {
      tooltip += `&nbsp;&nbsp;密度: ${values.density.toFixed(4)} m²/人<br/>`
    }
    if (values.peopleCount !== undefined) {
      tooltip += `&nbsp;&nbsp;人數: ${values.peopleCount} 人`
    }
    tooltip += `</div>`
  })

  return tooltip
}