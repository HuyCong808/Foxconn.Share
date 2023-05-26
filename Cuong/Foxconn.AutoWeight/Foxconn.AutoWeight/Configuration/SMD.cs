using Foxconn.AutoWeight.FoxconnEdit.OpenCV;
using static Foxconn.AutoWeight.Enums;

namespace Foxconn.AutoWeight.Configuration
{
    public class SMD
    {
        public int FOV_ID { get; set; }
        public int Id { get; set; }
        public string name { get; set; }
        public SMDAlgorithm Algorithm { get; set; }
        public SMDType SMDType { get; set; }
        public CvCodeRecognition CodeRecognition { get; set; }
        public CvTemplateMatching TemplateMatching { get; set; }
        public JRect ROI { get; set; }
        public SMD()
        {
            FOV_ID = 0;
            Id = -1;
            name = "";
            Algorithm = 0;
            SMDType = 0;
            CodeRecognition = new CvCodeRecognition();
            TemplateMatching = new CvTemplateMatching();
            ROI = new JRect(0, 0, 0, 0);
        }

    }
}
