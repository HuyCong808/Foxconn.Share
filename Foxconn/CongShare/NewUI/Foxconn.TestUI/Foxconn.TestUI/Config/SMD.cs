using Foxconn.TestUI.Enums;

namespace Foxconn.TestUI.Config
{
    public class SMD
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SMDType SMDType { get; set; }
        public Algorithm Algorithm { get; set; }
        public BRectangle ROI { get; set; }

        public SMD()
        {
            Id = -1;
            Name = "";
            SMDType = 0;
            Algorithm = 0;
            ROI = new BRectangle(0, 0, 0, 0);
        }
    }
}
