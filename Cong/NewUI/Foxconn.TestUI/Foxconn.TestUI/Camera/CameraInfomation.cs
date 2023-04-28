using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
