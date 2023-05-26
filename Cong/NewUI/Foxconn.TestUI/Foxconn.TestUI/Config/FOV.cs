using Foxconn.TestUI.Enums;
using System.Collections.Generic;

namespace Foxconn.TestUI.Config
{
    public class FOV
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string PathImage { get; set; }
        public string ExposureTime { get; set; }
        public FOVType FOVType { get; set; }
        public CameraMode CameraMode { get; set; }
        public List<SMD> SMDs { get; set; }
        public string A { get;set; }
        public FOV()
        {
            ID = -1;
            Name = "";
            ExposureTime = "";
            FOVType = 0;
            CameraMode = 0;
            SMDs = new List<SMD>();
        }
    }
}
