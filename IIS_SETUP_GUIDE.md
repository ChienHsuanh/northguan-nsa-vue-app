# IIS 設定指南 - Vue.js SPA 部署

## 前置需求檢查

### 1. 安裝必要組件

#### ASP.NET Core Hosting Bundle
```powershell
# 下載並安裝 ASP.NET Core 8.0 Hosting Bundle
# 從 Microsoft 官網下載: https://dotnet.microsoft.com/download/dotnet/8.0
# 安裝後重啟 IIS
iisreset
```

#### URL Rewrite Module
```powershell
# 從 Microsoft 官網下載並安裝 URL Rewrite Module 2.1
# https://www.iis.net/downloads/microsoft/url-rewrite
```

### 2. 驗證安裝
```powershell
# 檢查 ASP.NET Core Module 是否已安裝
Get-WindowsFeature -Name IIS-ASPNET47
Get-WindowsFeature -Name IIS-NetFxExtensibility45

# 檢查 .NET 版本
dotnet --list-runtimes
```

## IIS 網站設定步驟

### 步驟 1: 創建應用程式池

1. 開啟 **IIS 管理員**
2. 右鍵點擊 **應用程式集區** → **新增應用程式集區**
3. 設定如下：
   - **名稱**: `NorthguanNsaVueApp`
   - **.NET CLR 版本**: **沒有受控碼**
   - **受控管線模式**: **整合式**
4. 點擊 **確定**

### 步驟 2: 設定應用程式池進階設定

1. 右鍵點擊剛創建的應用程式池 → **進階設定**
2. 重要設定：
   - **處理序模型** → **身分識別**: `ApplicationPoolIdentity`
   - **處理序模型** → **閒置逾時 (分鐘)**: `0` (停用逾時)
   - **回收條件** → **定期時間間隔 (分鐘)**: `0` (停用定期回收)
   - **CPU** → **限制 (%)**: `0` (無限制)

### 步驟 3: 創建網站

1. 右鍵點擊 **網站** → **新增網站**
2. 設定如下：
   - **網站名稱**: `NorthguanNsaVueApp`
   - **應用程式集區**: 選擇剛創建的 `NorthguanNsaVueApp`
   - **實體路徑**: 指向您的部署目錄 (例如: `C:\inetpub\wwwroot\NorthguanNsaVueApp`)
   - **連接埠**: `80` (HTTP) 或 `443` (HTTPS)
   - **主機名稱**: 留空或設定您的域名

### 步驟 4: 設定目錄權限

```powershell
# 給予應用程式池身分適當權限
$appPoolName = "NorthguanNsaVueApp"
$sitePath = "C:\inetpub\wwwroot\NorthguanNsaVueApp"

# 給予 IIS_IUSRS 讀取權限
icacls $sitePath /grant "IIS_IUSRS:(OI)(CI)R" /T

# 給予應用程式池身分讀取權限
icacls $sitePath /grant "IIS AppPool\$appPoolName:(OI)(CI)R" /T

# 給予 logs 目錄寫入權限
$logsPath = "$sitePath\logs"
if (!(Test-Path $logsPath)) { New-Item -ItemType Directory -Path $logsPath }
icacls $logsPath /grant "IIS AppPool\$appPoolName:(OI)(CI)F" /T
```

## 部署文件結構

確保您的部署目錄結構如下：
```
C:\inetpub\wwwroot\NorthguanNsaVueApp\
├── northguan-nsa-vue-app.Server.dll
├── northguan-nsa-vue-app.Server.exe
├── web.config
├── appsettings.json
├── appsettings.Production.json
├── wwwroot/
│   ├── index.html
│   ├── assets/
│   │   ├── *.js
│   │   ├── *.css
│   │   └── *.woff2
│   └── images/
├── logs/ (空目錄，給應用程式寫入日誌)
└── 其他 .NET 相關文件
```

## 故障排除

### 問題 1: 500.19 錯誤 (配置錯誤)

**症狀**: HTTP 錯誤 500.19 - 內部伺服器錯誤
**原因**: web.config 配置問題或缺少必要模組

**解決方案**:
```powershell
# 1. 檢查 web.config 語法
# 2. 確認 URL Rewrite Module 已安裝
# 3. 檢查應用程式池設定
```

### 問題 2: 500.30 錯誤 (應用程式啟動失敗)

**症狀**: HTTP 錯誤 500.30 - ASP.NET Core 應用程式啟動失敗
**原因**: .NET 運行時問題或應用程式配置錯誤

**解決方案**:
```powershell
# 1. 檢查 stdout 日誌
Get-Content "C:\inetpub\wwwroot\NorthguanNsaVueApp\logs\stdout*.log" -Tail 50

# 2. 檢查事件檢視器
# Windows 日誌 → 應用程式

# 3. 手動測試應用程式
cd "C:\inetpub\wwwroot\NorthguanNsaVueApp"
dotnet northguan-nsa-vue-app.Server.dll
```

### 問題 3: 404 錯誤 (Vue 路由無法訪問)

**症狀**: 直接訪問 Vue 路由返回 404
**原因**: URL 重寫規則未生效

**解決方案**:
1. 確認 URL Rewrite Module 已安裝
2. 檢查 web.config 中的重寫規則
3. 確認 `index.html` 存在於 `wwwroot` 目錄

### 問題 4: 靜態文件無法載入

**症狀**: CSS、JS 文件返回 404 或 MIME 類型錯誤
**原因**: 靜態文件配置問題

**解決方案**:
```xml
<!-- 確認 web.config 中有正確的 MIME 類型設定 -->
<staticContent>
  <remove fileExtension=".js" />
  <mimeMap fileExtension=".js" mimeType="application/javascript" />
</staticContent>
```

## 診斷工具和命令

### 檢查 IIS 狀態
```powershell
# 檢查 IIS 服務狀態
Get-Service W3SVC, WAS

# 檢查應用程式池狀態
Get-IISAppPool

# 檢查網站狀態
Get-IISSite
```

### 檢查日誌
```powershell
# 檢查 IIS 日誌
Get-Content "C:\inetpub\logs\LogFiles\W3SVC1\*.log" -Tail 20

# 檢查應用程式日誌
Get-Content "C:\inetpub\wwwroot\NorthguanNsaVueApp\logs\stdout*.log" -Tail 20

# 檢查 Windows 事件日誌
Get-EventLog -LogName Application -Source "IIS*" -Newest 10
```
