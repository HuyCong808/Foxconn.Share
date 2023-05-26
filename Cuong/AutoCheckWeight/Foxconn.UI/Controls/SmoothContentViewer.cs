using System.Windows;
using System.Windows.Controls;

namespace Foxconn.UI.Controls
{
    public class SmoothContentViewer : Decorator
    {
        static SmoothContentViewer() => DefaultStyleKeyProperty.OverrideMetadata(typeof(SmoothContentViewer), new FrameworkPropertyMetadata(typeof(SmoothContentViewer)));

        protected override Size MeasureOverride(Size constraint)
        {
            Child?.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return new Size();
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Child?.Arrange(new Rect(0.0, 0.0, arrangeSize.Width, arrangeSize.Height));
            return arrangeSize;
        }
    }
}
