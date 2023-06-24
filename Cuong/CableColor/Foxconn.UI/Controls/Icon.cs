using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foxconn.UI.Controls
{
    public class Icon : Image
    {
        public static readonly DependencyProperty SourceBrushProperty = DependencyProperty.Register(nameof(SourceBrush), typeof(DrawingBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty RedChromaProperty = DependencyProperty.RegisterAttached(nameof(RedChroma), typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty GreenChromaProperty = DependencyProperty.RegisterAttached(nameof(GreenChroma), typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BlueChromaProperty = DependencyProperty.RegisterAttached(nameof(BlueChroma), typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.RegisterAttached("SelectedImage", typeof(ImageSource), typeof(Icon), new PropertyMetadata(null));
        public static readonly DependencyProperty DeselectedImageProperty = DependencyProperty.RegisterAttached("DeselectedImage", typeof(ImageSource), typeof(Icon), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedDrawingBrushProperty = DependencyProperty.RegisterAttached("SelectedDrawingBrush", typeof(DrawingBrush), typeof(Icon), new PropertyMetadata(null));
        public static readonly DependencyProperty DeselectedDrawingBrushProperty = DependencyProperty.RegisterAttached("DeselectedDrawingBrush", typeof(DrawingBrush), typeof(Icon), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowSelectedIconOnMouseOverProperty = DependencyProperty.RegisterAttached("ShowSelectedIconOnMouseOver", typeof(bool), typeof(Icon), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        static Icon() => StretchProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(Stretch.None));

        public DrawingBrush SourceBrush
        {
            get => (DrawingBrush)GetValue(SourceBrushProperty);
            set => SetValue(SourceBrushProperty, value);
        }

        public SolidColorBrush RedChroma
        {
            get => (SolidColorBrush)GetValue(RedChromaProperty);
            set => SetValue(RedChromaProperty, value);
        }

        public static SolidColorBrush GetRedChroma(DependencyObject obj) => (SolidColorBrush)obj.GetValue(RedChromaProperty);

        public static void SetRedChroma(DependencyObject obj, SolidColorBrush value) => obj.SetValue(RedChromaProperty, value);

        public SolidColorBrush GreenChroma
        {
            get => (SolidColorBrush)GetValue(GreenChromaProperty);
            set => SetValue(GreenChromaProperty, value);
        }

        public static SolidColorBrush GetGreenChroma(DependencyObject obj) => (SolidColorBrush)obj.GetValue(GreenChromaProperty);

        public static void SetGreenChroma(DependencyObject obj, SolidColorBrush value) => obj.SetValue(GreenChromaProperty, value);

        public SolidColorBrush BlueChroma
        {
            get => (SolidColorBrush)GetValue(BlueChromaProperty);
            set => SetValue(BlueChromaProperty, value);
        }

        public static SolidColorBrush GetBlueChroma(DependencyObject obj) => (SolidColorBrush)obj.GetValue(BlueChromaProperty);

        public static void SetBlueChroma(DependencyObject obj, SolidColorBrush value) => obj.SetValue(BlueChromaProperty, value);

        public static ImageSource GetSelectedImage(DependencyObject obj) => (ImageSource)obj.GetValue(SelectedImageProperty);

        public static void SetSelectedImage(DependencyObject obj, ImageSource value) => obj.SetValue(SelectedImageProperty, value);

        public static DrawingBrush GetSelectedDrawingBrush(DependencyObject obj) => (DrawingBrush)obj.GetValue(SelectedDrawingBrushProperty);

        public static void SetSelectedDrawingBrush(DependencyObject obj, DrawingBrush value) => obj.SetValue(SelectedDrawingBrushProperty, value);

        public static ImageSource GetDeselectedImage(DependencyObject obj) => (ImageSource)obj.GetValue(DeselectedImageProperty);

        public static void SetDeselectedImage(DependencyObject obj, ImageSource value) => obj.SetValue(DeselectedImageProperty, value);

        public static DrawingBrush GetDeselectedDrawingBrush(DependencyObject obj) => (DrawingBrush)obj.GetValue(DeselectedDrawingBrushProperty);

        public static void SetDeselectedDrawingBrush(DependencyObject obj, DrawingBrush value) => obj.SetValue(DeselectedDrawingBrushProperty, value);

        public static bool GetShowSelectedIconOnMouseOver(DependencyObject obj) => (bool)obj.GetValue(ShowSelectedIconOnMouseOverProperty);

        public static void SetShowSelectedIconOnMouseOver(DependencyObject obj, bool value) => obj.SetValue(ShowSelectedIconOnMouseOverProperty, value);

        protected override Size ArrangeOverride(Size finalSize) => Source == null ? finalSize : base.ArrangeOverride(finalSize);

        public static Point GetPixelSnappingOffset(Visual visual)
        {
            PresentationSource presentationSource = PresentationSource.FromVisual(visual);
            return presentationSource != null ? GetPixelSnappingOffset(visual, presentationSource.RootVisual) : new Point();
        }

        private static Point GetPixelSnappingOffset(Visual visual, Visual rootVisual)
        {
            Point point = new Point();
            if (rootVisual != null && visual.TransformToAncestor(rootVisual) is Transform ancestor && ancestor.Value.HasInverse)
                point = visual.PointFromScreen(visual.PointToScreen(point));
            return point;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            BitmapSource source = Source as BitmapSource;
            Rect rectangle = new Rect(0.0, 0.0, RenderSize.Width, RenderSize.Height);
            if (SourceBrush == null || source != null && IsClose(source.Width, RenderSize.Width) && IsClose(source.Height, RenderSize.Height))
            {
                ImageSource renderSource = RenderSource;
                if (renderSource == null)
                    return;
                drawingContext.DrawImage(renderSource, rectangle);
            }
            else
                drawingContext.DrawRectangle(RenderSourceBrush, null, rectangle);
        }

        private ImageSource RenderSource
        {
            get
            {
                if (Source == null)
                    return null;
                return RedChroma == null && GreenChroma == null && BlueChroma == null ? Source : ColorSwapper.SwapColors(Source, new ColorCallback(ConvertColor));
            }
        }

        private DrawingBrush RenderSourceBrush
        {
            get
            {
                if (SourceBrush == null)
                    return null;
                return RedChroma == null && GreenChroma == null && BlueChroma == null ? SourceBrush : (DrawingBrush)ColorSwapper.SwapColors(SourceBrush, new ColorCallback(ConvertColor));
            }
        }

        private Color ConvertColor(Color color)
        {
            if (color.R == color.G && color.R == color.B)
                return color;
            if (color.G == color.B && RedChroma != null)
                return ScaleColor(RedChroma.Color, color.R, color.G, color.A);
            if (color.R == color.B && GreenChroma != null)
                return ScaleColor(GreenChroma.Color, color.G, color.R, color.A);
            return color.R == color.G && BlueChroma != null ? ScaleColor(BlueChroma.Color, color.B, color.R, color.A) : color;
        }

        private Color ScaleColor(Color color, byte primary, byte white, byte alpha) => Color.FromArgb((byte)(alpha * color.A / byte.MaxValue), (byte)(color.R / (double)byte.MaxValue * (primary - white) + white), (byte)(color.G / (double)byte.MaxValue * (primary - white) + white), (byte)(color.B / (double)byte.MaxValue * (primary - white) + white));

        private static bool IsClose(double num1, double num2) => num1 > num2 * 0.9;
    }
}
