using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Foxconn.UI.Controls
{
    public static class AutoFocusBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(AutoFocusBehavior), new PropertyMetadata(new PropertyChangedCallback(IsEnabledPropertyChanged)));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void IsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement element))
                return;
            if (true.Equals(e.NewValue))
            {
                element.IsVisibleChanged += new DependencyPropertyChangedEventHandler(InputElement_IsVisibleChanged);
                Focus(element);
            }
            else
                element.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(InputElement_IsVisibleChanged);
        }

        private static void InputElement_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is UIElement element) || !true.Equals(e.NewValue))
                return;
            Focus(element);
        }

        private static void Focus(UIElement element)
        {
            if (Keyboard.FocusedElement == element)
                return;
            element.Dispatcher.BeginInvoke(new Action(() => Keyboard.Focus(element)), DispatcherPriority.Input);
        }
    }
}
