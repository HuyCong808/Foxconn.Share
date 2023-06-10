using Foxconn.TestUI.Enums;
using System;
using System.Collections.Generic;

namespace Foxconn.TestUI.Config
{
    public class FOV
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ImageBlockName { get; set; }
        public string ImagePath { get; set; }
        public int ExposureTime { get; set; }
        public bool IsEnable { get; set; }
        public CameraMode CameraMode { get; set; }
        public FOVType FOVType { get; set; }
        public List<SMD> SMDs { get; set; }
        public FOV()
        {
            ID = -1;
            Name = "";
            ImageBlockName = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            ImagePath = "";
            ExposureTime = 1000;
            IsEnable = true;
            CameraMode = 0;
            FOVType = 0;
            SMDs = new List<SMD>();
        }
    }
}
