using System.Xml.Serialization;

namespace Foxconn.Licensing
{
    [XmlRoot("WpaID")]
    public class WpaID
    {
        [XmlElement("Component")]
        public Component[] Components { get; set; }

        public class Component
        {
            [XmlAttribute("Key")]
            public string Key { get; set; }

            [XmlAttribute("Value")]
            public byte[] Value { get; set; }
        }
    }
}
