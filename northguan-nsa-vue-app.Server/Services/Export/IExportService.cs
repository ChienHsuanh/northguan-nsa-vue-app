namespace northguan_nsa_vue_app.Server.Services.Export
{
    /// <summary>
    /// 導出服務接口
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// 導出為Excel
        /// </summary>
        Task<byte[]> ExportToExcelAsync<T>(List<T> data, string sheetName, Dictionary<string, string> columnMappings);

        /// <summary>
        /// 導出為CSV
        /// </summary>
        Task<byte[]> ExportToCsvAsync<T>(List<T> data, Dictionary<string, string> columnMappings);

        /// <summary>
        /// 導出為PDF
        /// </summary>
        Task<byte[]> ExportToPdfAsync<T>(List<T> data, string title, Dictionary<string, string> columnMappings);
    }
}