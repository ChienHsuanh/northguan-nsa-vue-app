namespace northguan_nsa_vue_app.Server.Utils
{
    public static class CsvUtils
    {
        /// <summary>
        /// 逸脫 CSV 欄位，確保其符合 CSV 規範。
        /// </summary>
        /// <param name="field">要處理的字串。</param>
        /// <returns>處理後的字串，若為 null 則返回空字串。</returns>
        public static string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // 根據 RFC 4180 標準，如果欄位包含逗號、雙引號或換行符，則必須用雙引號括起來。
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                string escapedField = field.Replace("\"", "\"\"");
                return $"\"{escapedField}\"";
            }

            return field;
        }
    }
}