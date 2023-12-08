namespace Barunson.FamilyWeb.Models
{
    /// <summary>
    /// 룰렛 이벤트 Send Data
    /// </summary>
    public class RouletteEvent
    {
        public string eventCode { get; set; }
        public string key { get; set; }
        public string userName { get; set; }
        public string userPhone { get; set; }
        public string joinCorp { get; set; }
    }

    /// <summary>
    /// 룰렛 이벤트 Return Data
    /// </summary>
    public class RouletteResponseMessage
    {
        public int resultCode { get; set; }
        public string resultDate { get; set; }
        public string errorMsg { get; set; }
    }
}
