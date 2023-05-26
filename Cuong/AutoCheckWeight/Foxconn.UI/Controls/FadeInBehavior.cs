using System.Windows;
using System.Windows.Media.Animation;

namespace Foxconn.UI.Controls
{
    public static class FadeInBehavior
    {
        public static readonly DependencyProperty DurationProperty = DependencyProperty.RegisterAttached("Duration", typeof(Duration), typeof(FadeInBehavior));
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.RegisterAttached("Visibility", typeof(Visibility), typeof(FadeInBehavior), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(VisibilityPropertyChanged)));

        public static Duration GetDuration(DependencyObject obj) => (Duration)obj.GetValue(DurationProperty);

        public static void SetDuration(DependencyObject obj, Duration value) => obj.SetValue(DurationProperty, value);

        public static Visibility GetVisibility(DependencyObject obj) => (Visibility)obj.GetValue(VisibilityProperty);

        public static void SetVisibility(DependencyObject obj, Visibility value) => obj.SetValue(VisibilityProperty, value);

        private static void VisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uielement = d as UIElement;
            if (uielement == null)
                return;
            Duration duration = GetDuration(uielement);
            DoubleAnimation animation;
            if (Visibility.Visible.Equals(e.NewValue))
            {
                uielement.SetValue(UIElement.VisibilityProperty, e.NewValue);
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.Duration = duration;
                doubleAnimation.To = new double?(1.0);
                doubleAnimation.FillBehavior = FillBehavior.HoldEnd;
                animation = doubleAnimation;
            }
            else
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.Duration = duration;
                doubleAnimation.To = new double?(0.0);
                doubleAnimation.FillBehavior = FillBehavior.HoldEnd;
                animation = doubleAnimation;
                animation.Completed += (s, e2) => uielement.SetValue(UIElement.VisibilityProperty, e.NewValue);
            }
            animation.Freeze();
            uielement.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}
