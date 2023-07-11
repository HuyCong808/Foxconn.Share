using System.Windows;
using System.Windows.Controls;

namespace Foxconn.UI.Controls
{
    public static class OverlayTextBehavior
    {
        public static readonly DependencyProperty OverlayTextProperty = DependencyProperty.RegisterAttached("OverlayText", typeof(string), typeof(OverlayTextBehavior), new FrameworkPropertyMetadata(new PropertyChangedCallback(OverlayTextChanged)));
        public static readonly DependencyPropertyKey IsVisiblePropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsVisible", typeof(bool), typeof(OverlayTextBehavior), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty IsVisibleProperty = IsVisiblePropertyKey.DependencyProperty;

        private static void OverlayTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
                return;
            string newValue = e.NewValue as string;
            bool flag1 = !string.IsNullOrEmpty(e.OldValue as string);
            bool flag2 = !string.IsNullOrEmpty(newValue);
            if (flag2 == flag1)
                return;
            if (flag1)
            {
                textBox.TextChanged -= new TextChangedEventHandler(TextBox_TextChanged);
                textBox.ClearValue(IsVisiblePropertyKey);
            }
            if (!flag2)
                return;
            textBox.SetValue(IsVisiblePropertyKey, string.IsNullOrEmpty(textBox.Text));
            textBox.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox))
                return;
            textBox.SetValue(IsVisiblePropertyKey, string.IsNullOrEmpty(textBox.Text));
        }

        public static void SetOverlayText(DependencyObject d, string value) => d.SetValue(OverlayTextProperty, value);

        public static string GetOverlayText(DependencyObject d) => (string)d.GetValue(OverlayTextProperty);

        public static bool GetIsVisible(DependencyObject d) => (bool)d.GetValue(IsVisibleProperty);
    }
}
