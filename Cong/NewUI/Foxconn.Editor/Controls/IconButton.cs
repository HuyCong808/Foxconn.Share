using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Foxconn.AOI.Editor.Controls
{
    public class IconButton : Button
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Brush), typeof(IconButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        static IconButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
    }
}
