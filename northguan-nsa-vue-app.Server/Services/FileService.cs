using System.Text.RegularExpressions;

namespace northguan_nsa_vue_app.Server.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public FileService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public string GetUploadFileUrl(string folder, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return "";

            return $"/uploads/{folder}/{filename}";
        }

        public async Task<string> SaveUploadFileAsync(string folder, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("檔案不能為空");

            // Validate file size
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880); // 5MB default
            if (file.Length > maxFileSize)
                throw new ArgumentException($"檔案大小不能超過 {maxFileSize / 1024 / 1024}MB");

            // Validate file extension
            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException($"不支援的檔案格式: {fileExtension}");

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            // Create directory if not exists
            var basePath = _configuration["FileUpload:BasePath"] ?? "wwwroot/uploads";
            var uploadPath = Path.Combine(_environment.ContentRootPath, basePath, folder);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save file
            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<string> SaveBase64ImageAsync(string folder, string base64String, string? fileExtension = null)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                throw new ArgumentException("Base64 字串不能為空", nameof(base64String));

            try
            {
                // 移除 data:image/jpeg;base64, 前綴（如果存在）
                var cleanBase64 = CleanBase64String(base64String);

                // 轉換為位元組陣列
                var imageBytes = Convert.FromBase64String(cleanBase64);

                // 如果沒有指定副檔名，預設使用 .jpg
                var extension = fileExtension ?? ".jpg";
                if (!extension.StartsWith("."))
                    extension = "." + extension;

                // 產生唯一檔案名稱
                var fileName = $"{DateTime.Now:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..10]}{extension}";

                // 建立目錄路徑
                var basePath = _configuration["FileUpload:BasePath"] ?? "wwwroot/uploads";
                var uploadPath = Path.Combine(_environment.ContentRootPath, basePath, folder);

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // 儲存檔案
                var filePath = Path.Combine(uploadPath, fileName);
                await File.WriteAllBytesAsync(filePath, imageBytes);

                return fileName;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("無效的 Base64 格式", nameof(base64String), ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"儲存 Base64 圖片時發生錯誤：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清理 Base64 字串，移除前綴
        /// </summary>
        private static string CleanBase64String(string base64String)
        {
            // 移除 data:image/...;base64, 前綴
            var dataUrlPattern = @"^data:image\/[a-zA-Z]*;base64,";
            return Regex.Replace(base64String, dataUrlPattern, string.Empty);
        }

        public bool DeleteFile(string folder, string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                    return false;

                var basePath = _configuration["FileUpload:BasePath"] ?? "wwwroot/uploads";
                var filePath = Path.Combine(_environment.ContentRootPath, basePath, folder, filename);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}