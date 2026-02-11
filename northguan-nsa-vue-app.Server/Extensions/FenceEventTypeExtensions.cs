using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Extensions
{
    /// <summary>
    /// FenceEventType 枚舉的擴展方法
    /// </summary>
    public static class FenceEventTypeExtensions
    {
        /// <summary>
        /// 取得事件類型的中文描述
        /// </summary>
        /// <param name="eventType">事件類型</param>
        /// <returns>事件描述</returns>
        public static string GetDescription(this FenceEventType eventType)
        {
            return eventType switch
            {
                FenceEventType.Enter => "闖入事件",
                FenceEventType.Exit => "離開事件",
                _ => "未知事件"
            };
        }
    }
}