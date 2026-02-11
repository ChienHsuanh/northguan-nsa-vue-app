export const getStatusText = (status: string | undefined): string => {
  if (!status) {
    return "未知狀態";
  }

  const statusMap: { [key: string]: string } = {
    // 設備狀態
    online: "線上",
    offline: "離線",
    // 交通狀況
    red: "擁塞",
    yellow: "稍擠",
    green: "暢通",
  };
  return statusMap[status] || status;
};

export const getStatusClass = (status: string | undefined): string => {
  if (!status) {
    return "text-gray-500";
  }

  const classMap: { [key: string]: string } = {
    擁擠: "text-red-500",
    稍擠: "text-yellow-500",
    暢通: "text-green-500",
    red: "text-red-500",
    yellow: "text-yellow-500",
    green: "text-green-500",
  };
  return classMap[status] || "text-gray-500";
};

export const getStatusColor = (status: string): string => {
  const colorMap: { [key: string]: string } = {
    擁擠: "#ef4444",
    稍擠: "#eab308",
    暢通: "#22c55e",
    red: "#ef4444",
    yellow: "#eab308",
    green: "#22c55e",
  };
  return colorMap[status] || "#6b7280";
};

export const getParkingIcon = (status: string): string => {
  const iconMap: { [key: string]: string } = {
    擁擠: "/images/icons/parking-red.svg",
    稍擠: "/images/icons/parking-yellow.svg",
    暢通: "/images/icons/parking-green.svg",
    red: "/images/icons/parking-red.svg",
    yellow: "/images/icons/parking-yellow.svg",
    green: "/images/icons/parking-green.svg",
  };
  return iconMap[status] || "/images/icons/parking.svg";
};

export const getCrowdIcon = (status: string): string => {
  const iconMap: { [key: string]: string } = {
    擁擠: "/images/icons/crowd-red.svg",
    稍擠: "/images/icons/crowd-yellow.svg",
    暢通: "/images/icons/crowd-green.svg",
    red: "/images/icons/crowd-red.svg",
    yellow: "/images/icons/crowd-yellow.svg",
    green: "/images/icons/crowd-green.svg",
  };
  return iconMap[status] || "/images/icons/crowd.svg";
};

export const getTrafficIcon = (status: string): string => {
  const iconMap: { [key: string]: string } = {
    擁擠: "/images/icons/traffic-red.svg",
    稍擠: "/images/icons/traffic-yellow.svg",
    暢通: "/images/icons/traffic-green.svg",
    red: "/images/icons/traffic-red.svg",
    yellow: "/images/icons/traffic-yellow.svg",
    green: "/images/icons/traffic-green.svg",
  };
  return iconMap[status] || "/images/icons/traffic.svg";
};

export const isYouTubeUrl = (url: string): boolean => {
  return url.includes("youtube.com") || url.includes("youtu.be");
};

/**
 * 檢查 URL 是否為影片檔案
 * @param url 要檢查的 URL
 * @returns 如果是影片檔案則返回 true，否則返回 false
 */
export const isVideoFile = (url: string): boolean => {
  const videoExtensions = ['.mp4', '.avi', '.mov', '.wmv', '.flv', '.webm', '.mkv', '.m4v', '.3gp', '.ogv', '.m3u8', '.ts'];
  const urlLower = url.toLowerCase();
  return videoExtensions.some(ext => urlLower.endsWith(ext) || urlLower.includes(ext + '?'));
};

/**
 * 處理YouTube URL，轉換為無Cookie的嵌入格式
 * @param url 原始YouTube URL
 * @returns 處理後的嵌入URL
 */
export const processYouTubeUrl = (url: string): string => {
  if (!url) {
    return url;
  }

  // 檢查是否已為 nocookie 網域，如果是則直接回傳
  if (url.includes("youtube-nocookie.com")) {
    return url;
  }

  try {
    const urlObj = new URL(url);
    const host = urlObj.hostname;
    let videoId = "";
    let isLiveStream = false;

    // 處理不同網址格式，從中提取影片或頻道 ID
    if (host.includes("youtube.com") || host.includes("youtu.be")) {
      if (urlObj.pathname.includes("/live_stream")) {
        // 處理直播網址
        const channelId = urlObj.searchParams.get("channel");
        if (channelId) {
          videoId = `live_stream?channel=${channelId}`;
          isLiveStream = true;
        }
      } else if (urlObj.pathname.includes("/embed/")) {
        if (urlObj.searchParams.get("live_stream")) {
          // 處理嵌入直播頻道
          videoId = urlObj.searchParams.get("live_stream") || "";
          isLiveStream = true;
        } else {
          // 處理嵌入影片
          videoId = urlObj.searchParams.get("v") || urlObj.pathname.split("/")[2];
        }
      } else {
        // 處理一般影片網址
        videoId = urlObj.searchParams.get("v") || urlObj.pathname.split("/")[1];
      }
    }

    // 如果成功提取到 ID，則構建新的 nocookie 網址
    if (videoId) {
      // 構建通用的參數字串
      const params = new URLSearchParams({
        autoplay: "1",
        mute: "1",
        rel: "0",
        modestbranding: "1",
        controls: "1",
      }).toString();

      if (isLiveStream) {
        // 處理直播頻道
        return `https://www.youtube-nocookie.com/embed/${videoId}&${params}`;
      } else {
        // 處理一般影片
        return `https://www.youtube-nocookie.com/embed/${videoId}?${params}`;
      }
    }
  } catch (error) {
    console.error("無效的 URL:", url, error);
  }

  return url;
};
