const colorSets = [
  "#CCCCFF", "#7A0099", "#33FFFF", "#CC00CC", "#FF5511", "#00AA00", "#8C0044", "#FF0000",
  "#DDAA00", "#7744FF", "#FFCCCC", "#00AA88", "#FF7744", "#880000", "#FFCC22", "#4400CC",
  "#FF0088", "#CCBBFF", "#E38EFF", "#D1BBFF", "#BB5500", "#B088FF", "#FFFF77", "#008888",
  "#5555FF", "#00BBFF", "#7700FF", "#FF88C2", "#00DD77", "#9955FF", "#FFB3FF", "#A20055",
  "#9999FF", "#33FFAA", "#0000FF", "#FFB7DD", "#FFDDAA", "#E63F00", "#00AAAA", "#EEFFBB",
  "#99FF99", "#FF77FF", "#B94FFF", "#0066FF"
]

export class ColorUtil {
  static pickColorByOrder(order: number): string {
    return colorSets[order % colorSets.length]
  }

  static getColorSets(): string[] {
    return [...colorSets]
  }

  static getRandomColor(): string {
    const randomIndex = Math.floor(Math.random() * colorSets.length)
    return colorSets[randomIndex]
  }
}
