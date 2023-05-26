using Foxconn.TestUI.Enums;
using Foxconn.TestUI.OpenCV;
using Foxconn.TestUI.Editor;

namespace Foxconn.TestUI.Config
{
    public class SMD : NotifyProperty
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public SMDType SMDType { get; set; }

        public CvCodeRecognition CodeRecognition { get; set; }
        
        public CvTemplateMatching TemplateMatching { get; set; }

        public Algorithm Algorithm { get; set; }

        public BRectangle ROI { get; set; }
            
        public CvHSVExtraction HSVExtraction { get; set; }


        public SMD()
        {
            Id = -1;
            Name = "";
            SMDType = 0;
            Algorithm = 0;
            TemplateMatching = new CvTemplateMatching();
            CodeRecognition = new CvCodeRecognition();
            HSVExtraction = new CvHSVExtraction();
            ROI = new BRectangle(0, 0, 0, 0);
        }
    
    }
}
