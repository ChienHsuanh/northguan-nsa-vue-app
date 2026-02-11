# 北關 NSA Vue 應用系統 - 配置說明

## 📋 配置文件概覽

本項目使用 ASP.NET Core 的配置系統，支持多環境配置。主要配置文件包括：

- `appsettings.json` - 生產環境配置
- `appsettings.Development.json` - 開發環境配置

## ⚙️ 配置項目詳解

### 🔐 DefaultAdmin - 默認管理員配置

```json
{
  "DefaultAdmin": {
    "Email": "admin@northguan.com",
    "Username": "admin", 
    "Password": "NorthguanAdmin2024!",
    "Name": "北關系統管理員"
  }
}
```

| 配置項 | 說明 | 生產環境建議 |
|--------|------|-------------|
| `Email` | 管理員郵箱地址 | 使用真實的企業郵箱 |
| `Username` | 管理員用戶名 | 避免使用 "admin" 等常見用戶名 |
| `Password` | 管理員密碼 | 使用強密碼，定期更換 |
| `Name` | 管理員顯示名稱 | 設置有意義的名稱 |

**⚠️ 安全提醒**: 
- 生產環境中應使用環境變量或 Azure Key Vault 等安全方式存儲密碼
- 首次部署後應立即更改默認密碼

### 🗄️ Database - 數據庫配置

```json
{
  "Database": {
    "SeedSampleData": false,
    "AutoMigrate": true,
    "EnableHealthCheck": true
  }
}
```

| 配置項 | 說明 | 開發環境 | 生產環境 |
|--------|------|----------|----------|
| `SeedSampleData` | 是否生成示例數據 | `true` | `false` |
| `AutoMigrate` | 是否自動執行數據庫遷移 | `true` | `true` |
| `EnableHealthCheck` | 是否啟用健康檢查 | `true` | `true` |

### 📝 Logging - 日誌配置

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    },
    "LogAdminCredentials": false
  }
}
```

| 配置項 | 說明 | 開發環境 | 生產環境 |
|--------|------|----------|----------|
| `LogAdminCredentials` | 是否記錄管理員憑證 | `true` | `false` |
| `LogLevel.Default` | 默認日誌級別 | `Debug` | `Information` |
| `LogLevel.Microsoft.EntityFrameworkCore` | EF Core 日誌級別 | `Information` | `Warning` |

### 🔑 JwtSettings - JWT 配置

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "https://localhost",
    "Audience": "https://localhost", 
    "ExpirationInMinutes": 60
  }
}
```

| 配置項 | 說明 | 開發環境 | 生產環境 |
|--------|------|----------|----------|
| `ExpirationInMinutes` | Token 過期時間（分鐘） | `480` (8小時) | `60` (1小時) |
| `SecretKey` | JWT 簽名密鑰 | 開發用密鑰 | 強隨機密鑰 |
| `Issuer` | Token 發行者 | `https://localhost` | 實際域名 |
| `Audience` | Token 受眾 | `https://localhost` | 實際域名 |

### 📁 FileUpload - 文件上傳配置

```json
{
  "FileUpload": {
    "BasePath": "wwwroot/uploads",
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif"]
  }
}
```

| 配置項 | 說明 | 開發環境 | 生產環境 |
|--------|------|----------|----------|
| `BasePath` | 文件存儲路徑 | `wwwroot/dev-uploads` | `wwwroot/uploads` |
| `MaxFileSize` | 最大文件大小（字節） | `10485760` (10MB) | `5242880` (5MB) |
| `AllowedExtensions` | 允許的文件擴展名 | 更多類型 | 限制類型 |

### 🚀 Application - 應用程序配置

```json
{
  "Application": {
    "Name": "北關 NSA Vue 應用系統",
    "Version": "1.0.0",
    "Environment": "Production"
  }
}
```

### 🔧 Features - 功能開關（僅開發環境）

```json
{
  "Features": {
    "EnableSwagger": true,
    "EnableDetailedErrors": true,
    "EnableCors": true,
    "EnableSampleDataGeneration": true
  }
}
```

## 🌍 環境特定配置

### 開發環境特點
- ✅ 啟用詳細日誌記錄
- ✅ 自動生成示例數據
- ✅ 記錄管理員憑證（便於開發）
- ✅ 更長的 JWT 過期時間
- ✅ 更大的文件上傳限制
- ✅ 啟用 Swagger 文檔

### 生產環境特點
- 🛡️ 關閉敏感信息日誌
- 🛡️ 不生成示例數據
- 🛡️ 較短的 JWT 過期時間
- 🛡️ 嚴格的文件上傳限制
- 🛡️ 關閉開發工具

## 🔒 安全最佳實踐

### 1. 密碼管理
```bash
# 使用環境變量（推薦）
export DefaultAdmin__Password="YourSecurePassword123!"

# 或使用 Azure Key Vault
# 在 Program.cs 中配置 Key Vault
```

### 2. JWT 密鑰管理
```bash
# 生成強隨機密鑰
openssl rand -base64 64

# 設置環境變量
export JwtSettings__SecretKey="your-generated-key"
```

### 3. 數據庫連接字符串
```bash
# 使用環境變量
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=NorthguanDB;..."
```

## 📦 部署配置示例

### Docker 環境變量
```dockerfile
ENV DefaultAdmin__Email=admin@yourcompany.com
ENV DefaultAdmin__Password=YourSecurePassword123!
ENV Database__SeedSampleData=false
ENV JwtSettings__SecretKey=your-production-secret-key
```

### Azure App Service 配置
```json
{
  "DefaultAdmin:Email": "admin@yourcompany.com",
  "DefaultAdmin:Password": "YourSecurePassword123!",
  "Database:SeedSampleData": "false",
  "JwtSettings:SecretKey": "your-production-secret-key"
}
```

## 🔍 配置驗證

在應用程序啟動時，系統會自動驗證關鍵配置項：

1. ✅ 數據庫連接字符串
2. ✅ JWT 密鑰長度和強度
3. ✅ 管理員密碼複雜度
4. ✅ 文件上傳路徑權限

## 🆘 故障排除

### 常見問題

1. **數據庫連接失敗**
   - 檢查 `ConnectionStrings:DefaultConnection`
   - 確認 SQL Server 服務運行
   - 驗證數據庫權限

2. **JWT Token 無效**
   - 檢查 `JwtSettings:SecretKey` 長度
   - 確認 `Issuer` 和 `Audience` 設置
   - 驗證系統時間同步

3. **文件上傳失敗**
   - 檢查 `FileUpload:BasePath` 權限
   - 確認 `MaxFileSize` 設置
   - 驗證文件擴展名

### 日誌查看
```bash
# 查看應用程序日誌
tail -f logs/app.log

# 查看 EF Core 查詢日誌
grep "Microsoft.EntityFrameworkCore" logs/app.log
```

---

**📞 技術支持**: 如有配置問題，請聯繫開發團隊或查看項目文檔。