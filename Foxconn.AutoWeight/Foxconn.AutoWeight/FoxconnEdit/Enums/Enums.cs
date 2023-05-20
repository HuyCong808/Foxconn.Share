namespace Foxconn.AutoWeight
{
    public class Enums
    {
        public enum CameraType
        {
            Unknow,
            Basler,
            Baumer,
            Hikvision,
            Mindvision,
            Webcam
        }

        public enum CameraMode
        {
            Unknow,
            Top,
            Bottom
        }

        public enum FOVType
        {
            Unknow,
            SN,
            L1_CAP1,
            L1_CAP2,
            L1_PCB1,
            L1_PCB2,
            L1_SOLDER_CAP1,
            L1_SOLDER_CAP2,
            L2_CAP1,
            L2_CAP2,
            L2_PCB1,
            L2_PCB2,
            L2_SOLDER_CAP1,
            L2_SOLDER_CAP2,
            L1_CLEAR,
            L2_CLEAR
        }
        public enum SMDType
        {
            Unknow,
            SMD,
            Mark,
            Barcode,
            Others
        }
        public enum SMDAlgorithm
        {
            Unknow,
            CodeRecognition,
            HSVExtraction,
            TemplateMatching,
            Contour
        }

    }
}
