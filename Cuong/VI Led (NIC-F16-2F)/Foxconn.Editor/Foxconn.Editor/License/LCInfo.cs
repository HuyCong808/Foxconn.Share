namespace Foxconn.Editor
{
    public class LCInfo
    {
        [JsonEncrypt]
        public string Name { get; set; }

        [JsonEncrypt]
        public string Version { get; set; }

        [JsonEncrypt]
        public string Description { get; set; }

        [JsonEncrypt]
        public string Key { get; set; }

        [JsonEncrypt]
        public string Type { get; set; }

        [JsonEncrypt]
        public string Time { get; set; }

        public LCInfo()
        {
            Name = "FOXCONN";
            Version = "1.0.0.0";
            Description = "Not Available";
            Key = "T0902965789";
            Type = "Unknow";
            Time = System.DateTime.Now.AddDays(7).ToString("yyyy/MM/dd/ HH:mm:ss.ffff");
        }
    }

    public enum LicenseType
    {
        Unknow,
        Trial,
        Lifetime
    }
}
