using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Foxconn.UI.Controls
{
    public class EasingPointAnimation : PointAnimation
    {
        public EasingEquation Equation { get; set; }

        protected override Freezable CreateInstanceCore() => new EasingPointAnimation()
        {
            Equation = Equation
        };

        protected override Point GetCurrentValueCore(Point defaultOriginValue, Point defaultDestinationValue, AnimationClock animationClock)
        {
            TimeSpan timeSpan;
            double num;
            if (!animationClock.CurrentTime.HasValue)
            {
                num = 0.0;
            }
            else
            {
                timeSpan = animationClock.CurrentTime.Value;
                num = timeSpan.TotalMilliseconds;
            }
            double time = num;
            Point? nullable;
            Point point1;
            if (!From.HasValue)
            {
                point1 = defaultOriginValue;
            }
            else
            {
                nullable = From;
                point1 = nullable.Value;
            }
            Point point2 = point1;
            nullable = To;
            Point point3;
            if (!nullable.HasValue)
            {
                point3 = defaultDestinationValue;
            }
            else
            {
                nullable = To;
                point3 = nullable.Value;
            }
            Point point4 = point2;
            Vector vector = point3 - point4;
            timeSpan = Duration.TimeSpan;
            double totalMilliseconds = timeSpan.TotalMilliseconds;
            return new Point(CalculateCurrentValue(time, point2.X, vector.X, totalMilliseconds), CalculateCurrentValue(time, point2.Y, vector.Y, totalMilliseconds));
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
