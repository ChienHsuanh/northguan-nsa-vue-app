namespace northguan_nsa_vue_app.Server.Resources
{
    /// <summary>
    /// 驗證錯誤訊息資源類別
    /// </summary>
    public static class ValidationMessages
    {
        // 通用訊息
        public const string Required = "{0}為必填欄位";
        public const string StringLength = "{0}長度必須介於 {2} 到 {1} 個字元之間";
        public const string StringLengthMax = "{0}長度不能超過 {1} 個字元";
        public const string StringLengthMin = "{0}長度不能少於 {1} 個字元";
        public const string Range = "{0}必須介於 {1} 到 {2} 之間";
        public const string EmailAddress = "{0}格式不正確";
        public const string Phone = "{0}格式不正確";
        public const string RegularExpression = "{0}格式不正確";
        public const string Compare = "{0}與 {1} 不相符";
        public const string CreditCard = "{0}格式不正確";
        public const string Url = "{0}格式不正確";
        
        // 欄位名稱
        public static class FieldNames
        {
            public const string Username = "使用者名稱";
            public const string Password = "密碼";
            public const string Email = "電子郵件";
            public const string Name = "姓名";
            public const string Phone = "電話號碼";
            public const string EmployeeId = "員工編號";
            public const string Type = "類型";
            public const string StationID = "站點編號";
            public const string Lng = "經度";
            public const string Lat = "緯度";
            public const string Serial = "序號";
            public const string Area = "區域";
            public const string VideoUrl = "影片網址";
            public const string ApiUrl = "API網址";
            public const string NumberOfParking = "停車位數量";
            public const string City = "城市";
            public const string ETagNumber = "ETag號碼";
            public const string SpeedLimit = "速限";
            public const string ObservingTime = "觀測時間";
        }
        
        // 特定欄位的錯誤訊息
        public static class Specific
        {
            public const string UsernameRequired = "請輸入使用者名稱";
            public const string PasswordRequired = "請輸入密碼";
            public const string PasswordLength = "密碼長度必須介於 6 到 100 個字元之間";
            public const string EmailRequired = "請輸入電子郵件";
            public const string EmailFormat = "請輸入有效的電子郵件格式";
            public const string NameRequired = "請輸入姓名";
            public const string TypeRequired = "請選擇設備類型";
            public const string StationRequired = "請選擇站點";
            public const string SerialRequired = "請輸入設備序號";
            public const string CoordinateRequired = "請輸入座標";
            public const string CoordinateRange = "座標值超出有效範圍";
        }
    }
}