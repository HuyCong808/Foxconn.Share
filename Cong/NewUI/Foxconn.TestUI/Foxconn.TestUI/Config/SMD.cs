using Foxconn.TestUI.Editor;
using Foxconn.TestUI.Enums;
using Foxconn.TestUI.OpenCV;

namespace Foxconn.TestUI.Config
{
    public class SMD : NotifyProperty
    {
        public int FOV_ID { get; set; }
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public SMDType SMDType { get; set; }

        public CvCodeRecognition CodeRecognition { get; set; }


        public CvTemplateMatching TemplateMatching { get; set; }

        public Algorithm Algorithm { get; set; }

        public BRectangle ROI { get; set; }

        public CvHSVExtraction HSVExtraction { get; set; }

        public CvHSVExtractionQty HSVExtractionQty { get; set; }



        public SMD()
        {
            FOV_ID = 0;
            Id = -1;
            Name = "";
            IsEnabled = true;
            SMDType = 0;
            Algorithm = 0;
            TemplateMatching = new CvTemplateMatching();
            CodeRecognition = new CvCodeRecognition();
            HSVExtraction = new CvHSVExtraction();
            HSVExtractionQty = new CvHSVExtractionQty();
            ROI = new BRectangle(0, 0, 0, 0);
        }

    }
}
