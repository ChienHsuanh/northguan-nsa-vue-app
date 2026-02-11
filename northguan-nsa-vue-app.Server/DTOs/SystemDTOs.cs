namespace northguan_nsa_vue_app.Server.DTOs
{
    public class UpdateSystemSettingRequest
    {
        public required string SettingName { get; set; }
        public required string SettingValue { get; set; }
    }

    public class SystemSettingResponse
    {
        public required string Message { get; set; }
    }
}