using Foxconn.Editor.OpenCV;

namespace Foxconn.Editor.Configuration
{
    public class SMD
    {
        public int FOV_ID { get; set; }
        public int Id { get; set; }
        public string name { get; set; }
        public bool IsEnabled { get; set; }
        public SMDAlgorithm Algorithm { get; set; }
        public SMDType SMDType { get; set; }
        public CvCodeRecognition CodeRecognition { get; set; }
        public CvTemplateMatching TemplateMatching { get; set; }
        public CvHSVExtraction HSVExtraction { get; set; }
        public JRect ROI { get; set; }
        public SMD()
        {
            FOV_ID = 0;
            Id = -1;
            name = "";
            IsEnabled = true;
            Algorithm = 0;
            SMDType = 0;
            ROI = new JRect(0, 0, 0, 0);
            CodeRecognition = new CvCodeRecognition();
            TemplateMatching = new CvTemplateMatching();
            HSVExtraction = new CvHSVExtraction();

        }

    }
}
