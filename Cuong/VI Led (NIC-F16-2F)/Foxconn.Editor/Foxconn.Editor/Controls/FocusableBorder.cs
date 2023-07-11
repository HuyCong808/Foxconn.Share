using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Foxconn.Editor.Controls
{
    public class FocusableBorder : Border
    {
        static FocusableBorder() => FocusableProperty.OverrideMetadata(typeof(FocusableBorder), new FrameworkPropertyMetadata(true));

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
            base.OnMouseDown(e);
        }
    }
}
