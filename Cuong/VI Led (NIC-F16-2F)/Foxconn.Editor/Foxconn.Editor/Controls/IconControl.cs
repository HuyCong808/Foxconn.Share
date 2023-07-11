using System.Windows;
using System.Windows.Media;

namespace Foxconn.Editor.Controls
{
    public static class IconControl
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(Brush), typeof(IconControl));
        public static readonly DependencyProperty FillProperty = DependencyProperty.RegisterAttached("Fill", typeof(Brush), typeof(IconControl));
        public static readonly DependencyProperty DisabledBrushProperty = DependencyProperty.RegisterAttached("DisabledBrush", typeof(Brush), typeof(IconControl));
        public static readonly DependencyProperty MouseOverBrushProperty = DependencyProperty.RegisterAttached("MouseOverBrush", typeof(Brush), typeof(IconControl));
        public static readonly DependencyProperty PressedBrushProperty = DependencyProperty.RegisterAttached("PressedBrush", typeof(Brush), typeof(IconControl));
        public static readonly DependencyProperty Icon1Property = DependencyProperty.RegisterAttached("Icon1", typeof(ImageSource), typeof(IconControl));
        public static readonly DependencyProperty Icon2Property = DependencyProperty.RegisterAttached("Icon2", typeof(ImageSource), typeof(IconControl));
        public static readonly DependencyProperty Icon3Property = DependencyProperty.RegisterAttached("Icon3", typeof(ImageSource), typeof(IconControl));
        public static readonly DependencyProperty Icon4Property = DependencyProperty.RegisterAttached("Icon4", typeof(ImageSource), typeof(IconControl));

        public static Brush GetIcon(DependencyObject d)
        {
            return (Brush)d.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject d, Brush value)
        {
            d.SetValue(IconProperty, value);
        }

        public static Brush GetFill(DependencyObject d)
        {
            return (Brush)d.GetValue(FillProperty);
        }

        public static void SetFill(DependencyObject d, Brush value)
        {
            d.SetValue(FillProperty, value);
        }

        public static Brush GetDisabledBrush(DependencyObject d)
        {
            return (Brush)d.GetValue(DisabledBrushProperty);
        }

        public static void SetDisabledBrush(DependencyObject d, Brush value)
        {
            d.SetValue(DisabledBrushProperty, value);
        }

        public static Brush GetMouseOverBrush(DependencyObject d)
        {
            return (Brush)d.GetValue(MouseOverBrushProperty);
        }

        public static void SetMouseOverBrush(DependencyObject d, Brush value)
        {
            d.SetValue(MouseOverBrushProperty, value);
        }

        public static Brush GetPressedBrush(DependencyObject d)
        {
            return (Brush)d.GetValue(PressedBrushProperty);
        }

        public static void SetPressedBrush(DependencyObject d, Brush value)
        {
            d.SetValue(PressedBrushProperty, value);
        }

        public static ImageSource GetIcon1(DependencyObject d)
        {
            return (ImageSource)d.GetValue(Icon1Property);
        }

        public static void SetIcon1(DependencyObject d, ImageSource value)
        {
            d.SetValue(Icon1Property, value);
        }

        public static ImageSource GetIcon2(DependencyObject d)
        {
            return (ImageSource)d.GetValue(Icon2Property);
        }

        public static void SetIcon2(DependencyObject d, ImageSource value)
        {
            d.SetValue(Icon2Property, value);
        }

        public static ImageSource GetIcon3(DependencyObject d)
        {
            return (ImageSource)d.GetValue(Icon3Property);
        }

        public static void SetIcon3(DependencyObject d, ImageSource value)
        {
            d.SetValue(Icon3Property, value);
        }

        public static ImageSource GetIcon4(DependencyObject d)
        {
            return (ImageSource)d.GetValue(Icon4Property);
        }

        public static void SetIcon4(DependencyObject d, ImageSource value)
        {
            d.SetValue(Icon4Property, value);
        }
    }
}
