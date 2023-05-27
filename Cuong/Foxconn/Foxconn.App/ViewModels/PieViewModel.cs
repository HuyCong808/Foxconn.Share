using OxyPlot;
using OxyPlot.Series;

namespace Foxconn.App.ViewModels
{
    public class PieViewModel
    {
        public PlotModel Data { get; set; }

        public PieViewModel(int passNumber, int failNumber)
        {
            //Data = new PlotModel { Title = "Yield Rate" };
            Data = new PlotModel();

            dynamic pieSeries = new PieSeries { StrokeThickness = 1.0, InsideLabelPosition = 0.8, AngleSpan = 360, StartAngle = 0 };

            pieSeries.Slices.Add(new PieSlice("Pass", passNumber) { IsExploded = false, Fill = OxyColor.FromRgb(40, 205, 65) });
            pieSeries.Slices.Add(new PieSlice("Fail", failNumber) { IsExploded = false, Fill = OxyColor.FromRgb(255, 59, 48) });

            Data.Series.Add(pieSeries);
        }
    }
}
