namespace northguan_nsa_vue_app.Server.Services.ExternalApi
{
    /// <summary>
    /// 停車系統類型
    /// </summary>
    public enum ParkingSystemType
    {
        MP,  // MicroProgram (stables.com.tw)
        YP,  // YouParking (youparking.com.tw)
        NB,  // Nobel (nobel168.com.tw)
        AP,  // AltoB Parking
        NHR  // NHR System
    }

    /// <summary>
    /// 停車系統類型檢測器
    /// </summary>
    public static class ParkingSystemDetector
    {
        /// <summary>
        /// 根據 API URL 檢測停車系統類型
        /// </summary>
        public static ParkingSystemType DetectSystemType(string apiUrl)
        {
            if (string.IsNullOrEmpty(apiUrl))
            {
                return ParkingSystemType.MP; // 預設
            }

            if (apiUrl.Contains("youparking.com.tw"))
            {
                return ParkingSystemType.YP;
            }

            if (apiUrl.Contains("nobel168.com.tw"))
            {
                return ParkingSystemType.NB;
            }

            if (apiUrl.Contains("parking/altobParking"))
            {
                return ParkingSystemType.AP;
            }

            if (apiUrl.Contains("stables.com.tw"))
            {
                return ParkingSystemType.MP;
            }

            if (apiUrl.Contains("/nhr/"))
            {
                return ParkingSystemType.NHR;
            }

            return ParkingSystemType.MP; // 預設為 MP 系統
        }

        /// <summary>
        /// 獲取系統類型的顯示名稱
        /// </summary>
        public static string GetSystemTypeName(ParkingSystemType systemType)
        {
            return systemType switch
            {
                ParkingSystemType.MP => "MicroProgram",
                ParkingSystemType.YP => "YouParking",
                ParkingSystemType.NB => "Nobel",
                ParkingSystemType.AP => "AltoB Parking",
                ParkingSystemType.NHR => "NHR System",
                _ => "Unknown"
            };
        }
    }
}