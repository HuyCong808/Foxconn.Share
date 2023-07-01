using System.Windows;
using System.Windows.Media;

namespace Foxconn.AOI.Editor.Controls
{
    public class Icon : FrameworkElement
    {
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(nameof(IconBrush), typeof(Brush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        public Brush IconBrush
        {
            get => (Brush)GetValue(IconBrushProperty);
            set => SetValue(IconBrushProperty, value);
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Brush iconBrush = IconBrush;
            if (iconBrush == null)
                return;
            drawingContext.PushOpacityMask(iconBrush);
            drawingContext.DrawRectangle(Fill, null, new Rect(RenderSize));
            drawingContext.Pop();
        }
    }
}