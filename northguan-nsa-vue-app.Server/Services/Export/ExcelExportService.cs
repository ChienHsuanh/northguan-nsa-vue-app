using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Reflection;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace northguan_nsa_vue_app.Server.Services.Export
{
    /// <summary>
    /// 優化的Excel導出服務
    /// </summary>
    public class ExcelExportService : IExportService
    {
        // 緩存屬性信息和編譯過的正則表達式
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
        private static readonly Regex _htmlTagRegex = new("<.*?>", RegexOptions.Compiled);

        public async Task<byte[]> ExportToExcelAsync<T>(List<T> data, string sheetName, Dictionary<string, string> columnMappings)
        {
            return await Task.Run(() =>
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet(sheetName);

                // 創建標題行
                CreateHeaderRowOptimized(sheet, columnMappings);

                // 創建數據行 - 優化版本
                CreateDataRowsOptimized(sheet, data, columnMappings);

                // 轉換為字節數組
                using var memoryStream = new MemoryStream();
                workbook.Write(memoryStream, false);
                return memoryStream.ToArray();
            });
        }

        public Task<byte[]> ExportToCsvAsync<T>(List<T> data, Dictionary<string, string> columnMappings)
        {
            // CSV導出實現
            throw new NotImplementedException("CSV export not implemented yet");
        }

        public Task<byte[]> ExportToPdfAsync<T>(List<T> data, string title, Dictionary<string, string> columnMappings)
        {
            // PDF導出實現
            throw new NotImplementedException("PDF export not implemented yet");
        }

        private void CreateHeaderRowOptimized(ISheet sheet, Dictionary<string, string> columnMappings)
        {
            var headerRow = sheet.CreateRow(0);

            // 創建單一樣式供所有標題單元格使用
            var headerCellStyle = sheet.Workbook.CreateCellStyle();
            var headerFont = sheet.Workbook.CreateFont();
            headerFont.IsBold = true;
            headerCellStyle.SetFont(headerFont);

            var cellIndex = 0;
            foreach (var mapping in columnMappings)
            {
                var cell = headerRow.CreateCell(cellIndex++);
                cell.SetCellValue(mapping.Value);
                cell.CellStyle = headerCellStyle; // 重用樣式對象
            }
        }

        private void CreateDataRowsOptimized<T>(ISheet sheet, List<T> data, Dictionary<string, string> columnMappings)
        {
            if (data == null || !data.Any()) return;

            var type = typeof(T);

            // 使用緩存的屬性信息
            var properties = _propertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            // 預先映射屬性並創建快速查找字典
            var propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
            {
                propertyMap[prop.Name] = prop;
            }

            // 創建有序的屬性列表和快速訪問器
            var orderedProperties = columnMappings.Keys
                .Select(key => propertyMap.GetValueOrDefault(key))
                .ToArray(); // 使用數組而非List提升性能

            // 預先編譯屬性訪問器 (可選 - 對於非常大的數據集)
            var propertyGetters = orderedProperties
                .Select(prop => prop != null ? CreatePropertyGetter<T>(prop) : null)
                .ToArray();

            // 批量創建行 - 減少內存分配
            var rowIndex = 1;
            foreach (var item in data)
            {
                var dataRow = sheet.CreateRow(rowIndex++);

                // 優化的單元格創建
                for (int cellIndex = 0; cellIndex < orderedProperties.Length; cellIndex++)
                {
                    var cell = dataRow.CreateCell(cellIndex);
                    var prop = orderedProperties[cellIndex];

                    if (prop != null)
                    {
                        // 使用預編譯的getter (可選優化)
                        var getter = propertyGetters[cellIndex];
                        var value = getter?.Invoke(item) ?? prop.GetValue(item);
                        SetCellValueOptimized(cell, value);
                    }
                    else
                    {
                        cell.SetCellValue(string.Empty);
                    }
                }
            }
        }

        // 可選：創建屬性訪問器以提升大數據集性能
        private static Func<T, object?>? CreatePropertyGetter<T>(PropertyInfo prop)
        {
            try
            {
                return (Func<T, object?>)Delegate.CreateDelegate(
                    typeof(Func<T, object?>),
                    prop.GetGetMethod());
            }
            catch
            {
                // 如果無法創建委託，返回null使用反射
                return null;
            }
        }

        private void SetCellValueOptimized(ICell cell, object? value)
        {
            if (value == null)
            {
                cell.SetCellValue(string.Empty);
                return;
            }

            // 使用模式匹配提升性能
            switch (value)
            {
                case string stringValue when string.IsNullOrEmpty(stringValue):
                    cell.SetCellValue(string.Empty);
                    break;

                case string stringValue:
                    // 使用預編譯的正則表達式
                    var cleanString = _htmlTagRegex.Replace(stringValue, string.Empty);
                    cell.SetCellValue(cleanString);
                    break;

                case int intValue:
                    cell.SetCellValue(intValue);
                    break;

                case long longValue:
                    cell.SetCellValue(longValue);
                    break;

                case float floatValue:
                    cell.SetCellValue(floatValue);
                    break;

                case double doubleValue:
                    cell.SetCellValue(doubleValue);
                    break;

                case decimal decimalValue:
                    cell.SetCellValue((double)decimalValue);
                    break;

                case DateTime dateTimeValue:
                    // 使用數值格式而非字符串格式以提升性能
                    cell.SetCellValue(dateTimeValue);

                    // 可選：設置日期格式
                    var dateStyle = cell.Sheet.Workbook.CreateCellStyle();
                    var dateFormat = cell.Sheet.Workbook.CreateDataFormat();
                    dateStyle.DataFormat = dateFormat.GetFormat("yyyy-MM-dd HH:mm:ss");
                    cell.CellStyle = dateStyle;
                    break;

                case DateTimeOffset dateTimeOffsetValue:
                    cell.SetCellValue(dateTimeOffsetValue.DateTime);
                    break;

                case bool boolValue:
                    cell.SetCellValue(boolValue ? "是" : "否");
                    break;

                case Enum enumValue:
                    cell.SetCellValue(enumValue.ToString());
                    break;

                default:
                    var defaultStringValue = value.ToString();
                    if (!string.IsNullOrEmpty(defaultStringValue))
                    {
                        // 使用預編譯的正則表達式
                        var cleanValue = _htmlTagRegex.Replace(defaultStringValue, string.Empty);
                        cell.SetCellValue(cleanValue);
                    }
                    else
                    {
                        cell.SetCellValue(string.Empty);
                    }
                    break;
            }
        }

        // 可選：如果需要設置列寬，提供批量設置方法
        private void SetColumnWidths(ISheet sheet, Dictionary<int, int> columnWidths)
        {
            foreach (var kvp in columnWidths)
            {
                sheet.SetColumnWidth(kvp.Key, kvp.Value);
            }
        }

        // 可選：批量處理大數據集的方法
        public async Task<byte[]> ExportLargeDatasetAsync<T>(
            IEnumerable<T> data,
            string sheetName,
            Dictionary<string, string> columnMappings,
            int batchSize = 10000)
        {
            return await Task.Run(() =>
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet(sheetName);

                CreateHeaderRowOptimized(sheet, columnMappings);

                var rowIndex = 1;
                var batch = new List<T>(batchSize);

                foreach (var item in data)
                {
                    batch.Add(item);

                    if (batch.Count >= batchSize)
                    {
                        ProcessBatch(sheet, batch, columnMappings, ref rowIndex);
                        batch.Clear();

                        // 可選：強制垃圾收集以控制內存使用
                        if (rowIndex % (batchSize * 5) == 0)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                }

                // 處理剩餘的數據
                if (batch.Count > 0)
                {
                    ProcessBatch(sheet, batch, columnMappings, ref rowIndex);
                }

                using var memoryStream = new MemoryStream();
                workbook.Write(memoryStream, false);
                return memoryStream.ToArray();
            });
        }

        private void ProcessBatch<T>(ISheet sheet, List<T> batch, Dictionary<string, string> columnMappings, ref int rowIndex)
        {
            var type = typeof(T);
            var properties = _propertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            var propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
            {
                propertyMap[prop.Name] = prop;
            }

            var orderedProperties = columnMappings.Keys
                .Select(key => propertyMap.GetValueOrDefault(key))
                .ToArray();

            foreach (var item in batch)
            {
                var dataRow = sheet.CreateRow(rowIndex++);

                for (int cellIndex = 0; cellIndex < orderedProperties.Length; cellIndex++)
                {
                    var cell = dataRow.CreateCell(cellIndex);
                    var prop = orderedProperties[cellIndex];

                    if (prop != null)
                    {
                        var value = prop.GetValue(item);
                        SetCellValueOptimized(cell, value);
                    }
                    else
                    {
                        cell.SetCellValue(string.Empty);
                    }
                }
            }
        }
    }
}