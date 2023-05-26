using System;
using System.Collections.Generic;
using static Foxconn.AutoWeight.Enums;

namespace Foxconn.AutoWeight.Configuration
{
    public class FOV
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ImageBlockName { get; set; }
        public int ExposureTime { get; set; }
        public CameraMode CameraMode { get; set; }
        public FOVType FOVType { get; set; }
        public List<SMD> SMDs { get; set; }

        public FOV()
        {
            ID = -1;
            Name = "";
            ImageBlockName = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            ExposureTime = 0;
            CameraMode = 0;
            FOVType = 0;
            SMDs = new List<SMD>();

        }


    }
}
