using System.Windows;
using System.Windows.Media;

namespace Foxconn.UI.Controls
{
    public static class IconBehavior
    {
        public static readonly DependencyProperty NormalDrawingBrushProperty = DependencyProperty.RegisterAttached("NormalDrawingBrush", typeof(DrawingBrush), typeof(IconBehavior), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty MouseOverDrawingBrushProperty = DependencyProperty.RegisterAttached("MouseOverDrawingBrush", typeof(DrawingBrush), typeof(IconBehavior), new FrameworkPropertyMetadata(null));

        public static DrawingBrush GetNormalDrawingBrush(DependencyObject obj) => (DrawingBrush)obj.GetValue(NormalDrawingBrushProperty);

        public static void SetNormalDrawingBrush(DependencyObject obj, DrawingBrush value) => obj.SetValue(NormalDrawingBrushProperty, value);

        public static DrawingBrush GetMouseOverDrawingBrush(DependencyObject obj) => (DrawingBrush)obj.GetValue(MouseOverDrawingBrushProperty);

        public static void SetMouseOverDrawingBrush(DependencyObject obj, DrawingBrush value) => obj.SetValue(MouseOverDrawingBrushProperty, value);
    }
}
