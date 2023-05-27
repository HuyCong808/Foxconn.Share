namespace Foxconn.App.Controllers.Image
{
    public class AlignmentImage
    {
        public class Deviation
        {
            /// <summary>
            /// Deivation dx
            /// </summary>
            public double dx { get; set; }
            /// <summary>
            /// Deviation dy
            /// </summary>
            public double dy { get; set; }
            /// <summary>
            /// Deviation angle
            /// </summary>
            public double dw { get; set; }
            public Deviation()
            {
                dx = 0;
                dy = 0;
                dw = 0;
            }
            public Deviation Clone()
            {
                return new Deviation()
                {
                    dx = this.dx,
                    dy = this.dy,
                    dw = this.dw
                };
            }
        }
    }
}
