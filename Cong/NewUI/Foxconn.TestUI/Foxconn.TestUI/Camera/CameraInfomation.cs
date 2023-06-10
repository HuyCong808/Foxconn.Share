using System;

namespace Foxconn.TestUI.Camera
{
    public class CameraInformation : ICloneable
    {
        public string UserDefinedName { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
