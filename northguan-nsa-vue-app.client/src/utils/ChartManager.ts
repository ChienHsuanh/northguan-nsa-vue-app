import { Chart, type ChartConfiguration, type ChartType, registerables } from "chart.js";
import { ColorUtil } from "@/utils/colorUtil";

Chart.register(...registerables);

export interface ChartDataset {
  label: string;
  data: (number | null)[];
  backgroundColor?: string;
  borderColor?: string;
  hidden?: boolean;
}

export interface ChartOptions {
  showLegend?: boolean;
  yLabels?: Record<number, string>;
  responsive?: boolean;
  maintainAspectRatio?: boolean;
}

export const getDefaultLineWithLabelAndData = (
  label: string,
  data: (number | null)[],
  counter: number
): ChartDataset => {
  return {
    label,
    data,
    backgroundColor: "rgba(255,0,0,0)",
    borderColor: ColorUtil.pickColorByOrder(counter),
  };
};

export class ChartManager {
  private chartObj: Chart | null = null;

  constructor(
    divID: string,
    type: ChartType,
    labels: string[],
    datasets: ChartDataset[],
    customOption: ChartOptions = {}
  ) {
    const option: ChartOptions = {
      showLegend: false,
      yLabels: undefined,
      responsive: true,
      maintainAspectRatio: false,
      ...customOption,
    };

    this.initChart(divID, type, labels, datasets, option);
  }

  private async initChart(
    divID: string,
    type: ChartType,
    labels: string[],
    datasets: ChartDataset[],
    option: ChartOptions
  ): Promise<void> {
    // 等待 DOM 元素準備就緒
    let retryCount = 0;
    const maxRetries = 10;

    const tryCreateCanvas = (): void => {
      if (retryCount >= maxRetries) {
        console.error(`Failed to create chart after ${maxRetries} retries for element: ${divID}`);
        return;
      }

      const element = document.getElementById(divID);
      if (element) {
        // 如果是 div 元素，創建 canvas 子元素
        let canvas: HTMLCanvasElement;
        if (element.tagName.toLowerCase() === "canvas") {
          canvas = element as HTMLCanvasElement;
        } else {
          // 清空 div 內容並創建 canvas
          element.innerHTML = "";
          canvas = document.createElement("canvas");
          canvas.style.width = "100%";
          canvas.style.height = "100%";
          element.appendChild(canvas);
        }

        const ctx = canvas.getContext("2d");
        if (ctx) {
          const config: ChartConfiguration = {
            type,
            data: {
              labels,
              datasets: datasets.map((dataset) => ({
                ...dataset,
                tension: 0.4, // 更新的 Chart.js 語法
              })),
            },
            options: {
              plugins: {
                legend: {
                  display: option.showLegend || false,
                },
              },
              animation: {
                duration: 0,
              },
              scales: {
                y: {
                  ticks: {
                    callback: function (value) {
                      if (option.yLabels) {
                        return option.yLabels[value as number];
                      }
                      // 只顯示整數
                      return Math.floor(value as number) === value ? value : "";
                    },
                  },
                },
              },
              responsive: option.responsive !== false,
              maintainAspectRatio: option.maintainAspectRatio === true,
            },
          };

          this.chartObj = new Chart(ctx, config);
        } else {
          console.error(`Failed to get 2D context for canvas in element: ${divID}`);
        }
      } else {
        retryCount++;
        setTimeout(tryCreateCanvas, 100);
      }
    };

    tryCreateCanvas();
  }

  clear(): void {
    this.chartObj?.clear();
  }

  render(): void {
    this.chartObj?.render();
  }

  destroy(): void {
    this.chartObj?.destroy();
    this.chartObj = null;
  }

  update(labels: string[], datasets: ChartDataset[]): void {
    if (!this.chartObj) return;

    const datasetMap: Record<string, ChartDataset> = {};
    datasets.forEach((item) => {
      datasetMap[item.label] = item;
    });

    // 更新現有數據集
    this.chartObj.data.datasets.forEach((dataset) => {
      const newData = datasetMap[dataset.label as string];
      if (newData) {
        dataset.data = newData.data;
      }
    });

    this.chartObj.data.labels = labels;
    this.chartObj.update();
  }

  getChart(): Chart | null {
    return this.chartObj;
  }
}
