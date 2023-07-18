using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Foxconn.UI.Controls
{
    public class EasingDoubleAnimation : DoubleAnimation
    {
        public EasingEquation Equation { get; set; }

        protected override Freezable CreateInstanceCore() => new EasingDoubleAnimation()
        {
            Equation = Equation
        };

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            TimeSpan timeSpan;
            double num1;
            if (!animationClock.CurrentTime.HasValue)
            {
                num1 = 0.0;
            }
            else
            {
                timeSpan = animationClock.CurrentTime.Value;
                num1 = timeSpan.TotalMilliseconds;
            }
            double time = num1;
            double? nullable;
            double num2;
            if (!From.HasValue)
            {
                num2 = defaultOriginValue;
            }
            else
            {
                nullable = From;
                num2 = nullable.Value;
            }
            double from = num2;
            nullable = To;
            double num3;
            if (!nullable.HasValue)
            {
                num3 = defaultDestinationValue;
            }
            else
            {
                nullable = To;
                num3 = nullable.Value;
            }
            double num4 = from;
            double delta = num3 - num4;
            timeSpan = Duration.TimeSpan;
            double totalMilliseconds = timeSpan.TotalMilliseconds;
            return CalculateCurrentValue(time, from, delta, totalMilliseconds);
        }

        private double CalculateCurrentValue(double time, double from, double delta, double duration)
        {
            switch (Equation)
            {
                case EasingEquation.Linear:
                    return EasingEquations.Linear(time, from, delta, duration);
                case EasingEquation.BackEaseIn:
                    return EasingEquations.EaseInBack(time, from, delta, duration);
                case EasingEquation.BackEaseInOut:
                    return EasingEquations.EaseInOutBack(time, from, delta, duration);
                case EasingEquation.BackEaseOut:
                    return EasingEquations.EaseOutBack(time, from, delta, duration);
                case EasingEquation.BounceEaseIn:
                    return EasingEquations.EaseInBounce(time, from, delta, duration);
                case EasingEquation.BounceEaseInOut:
                    return EasingEquations.EaseInOutBounce(time, from, delta, duration);
                case EasingEquation.BounceEaseOut:
                    return EasingEquations.EaseOutBounce(time, from, delta, duration);
                case EasingEquation.CircEaseIn:
                    return EasingEquations.EaseInCirc(time, from, delta, duration);
                case EasingEquation.CircEaseInOut:
                    return EasingEquations.EaseInOutCirc(time, from, delta, duration);
                case EasingEquation.CircEaseOut:
                    return EasingEquations.EaseOutCirc(time, from, delta, duration);
                case EasingEquation.CubicEaseIn:
                    return EasingEquations.EaseInCubic(time, from, delta, duration);
                case EasingEquation.CubicEaseInOut:
                    return EasingEquations.EaseInOutCirc(time, from, delta, duration);
                case EasingEquation.CubicEaseOut:
                    return EasingEquations.EaseOutCirc(time, from, delta, duration);
                case EasingEquation.ElasticEaseIn:
                    return EasingEquations.EaseInElastic(time, from, delta, duration);
                case EasingEquation.ElasticEaseInOut:
                    return EasingEquations.EaseInOutElastic(time, from, delta, duration);
                case EasingEquation.ElasticEaseOut:
                    return EasingEquations.EaseOutElastic(time, from, delta, duration);
                case EasingEquation.ExpoEaseIn:
                    return EasingEquations.EaseInExpo(time, from, delta, duration);
                case EasingEquation.ExpoEaseInOut:
                    return EasingEquations.EaseInOutElastic(time, from, delta, duration);
                case EasingEquation.ExpoEaseOut:
                    return EasingEquations.EaseOutElastic(time, from, delta, duration);
                case EasingEquation.QuadEaseIn:
                    return EasingEquations.EaseInQuad(time, from, delta, duration);
                case EasingEquation.QuadEaseInOut:
                    return EasingEquations.EaseInOutQuad(time, from, delta, duration);
                case EasingEquation.QuadEaseOut:
                    return EasingEquations.EaseOutQuad(time, from, delta, duration);
                case EasingEquation.QuartEaseIn:
                    return EasingEquations.EaseInQuart(time, from, delta, duration);
                case EasingEquation.QuartEaseInOut:
                    return EasingEquations.EaseInOutQuart(time, from, delta, duration);
                case EasingEquation.QuartEaseOut:
                    return EasingEquations.EaseOutQuart(time, from, delta, duration);
                case EasingEquation.QuintEaseIn:
                    return EasingEquations.EaseInQuint(time, from, delta, duration);
                case EasingEquation.QuintEaseInOut:
                    return EasingEquations.EaseInOutQuint(time, from, delta, duration);
                case EasingEquation.QuintEaseOut:
                    return EasingEquations.EaseOutQuint(time, from, delta, duration);
                case EasingEquation.SineEaseIn:
                    return EasingEquations.EaseInSine(time, from, delta, duration);
                case EasingEquation.SineEaseInOut:
                    return EasingEquations.EaseInOutSine(time, from, delta, duration);
                case EasingEquation.SineEaseOut:
                    return EasingEquations.EaseOutSine(time, from, delta, duration);
                default:
                    return double.MinValue;
            }
        }
    }
}
