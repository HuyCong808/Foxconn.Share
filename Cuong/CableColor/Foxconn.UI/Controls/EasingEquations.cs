using System;

namespace Foxconn.UI.Controls
{
    public static class EasingEquations
    {
        public static double EaseOutElastic(double time, double startVal, double newVal, double duration)
        {
            if ((time /= duration) == 1.0)
                return startVal + newVal;
            double num1 = duration * 0.3;
            double num2 = num1 / 4.0;
            return newVal * Math.Pow(2.0, -10.0 * time) * Math.Sin((time * duration - num2) * (2.0 * Math.PI) / num1) + newVal + startVal;
        }

        public static double EaseInElastic(double time, double startVal, double newVal, double duration)
        {
            if ((time /= duration) == 1.0)
                return startVal + newVal;
            double num1 = duration * 0.3;
            double num2 = num1 / 4.0;
            return -(newVal * Math.Pow(2.0, 10.0 * --time) * Math.Sin((time * duration - num2) * (2.0 * Math.PI) / num1)) + startVal;
        }

        public static double EaseInOutElastic(double time, double startVal, double newVal, double duration)
        {
            if ((time /= duration / 2.0) == 2.0)
                return startVal + newVal;
            double num1 = duration * (9.0 / 20.0);
            double num2 = num1 / 4.0;
            return time < 1.0 ? -0.5 * (newVal * Math.Pow(2.0, 10.0 * --time) * Math.Sin((time * duration - num2) * (2.0 * Math.PI) / num1)) + startVal : newVal * Math.Pow(2.0, -10.0 * --time) * Math.Sin((time * duration - num2) * (2.0 * Math.PI) / num1) * 0.5 + newVal + startVal;
        }

        public static double EaseOutBounce(double time, double startVal, double newVal, double duration)
        {
            if ((time /= duration) < 4.0 / 11.0)
                return newVal * (121.0 / 16.0 * time * time) + startVal;
            if (time < 8.0 / 11.0)
                return newVal * (121.0 / 16.0 * (time -= 6.0 / 11.0) * time + 0.75) + startVal;
            return time < 10.0 / 11.0 ? newVal * (121.0 / 16.0 * (time -= 9.0 / 11.0) * time + 15.0 / 16.0) + startVal : newVal * (121.0 / 16.0 * (time -= 21.0 / 22.0) * time + 63.0 / 64.0) + startVal;
        }

        public static double EaseInBounce(double time, double startVal, double newVal, double duration)
        {
            if ((time /= duration) < 4.0 / 11.0)
                return newVal * (121.0 / 16.0 * time * time) + startVal;
            if (time < 8.0 / 11.0)
                return newVal * (121.0 / 16.0 * (time -= 6.0 / 11.0) * time + 0.75) + startVal;
            return time < 10.0 / 11.0 ? newVal * (121.0 / 16.0 * (time -= 9.0 / 11.0) * time + 15.0 / 16.0) + startVal : newVal * (121.0 / 16.0 * (time -= 21.0 / 22.0) * time + 63.0 / 64.0) + startVal;
        }

        public static double EaseInOutBounce(double time, double startVal, double newVal, double duration)
        {
            return time < duration / 2.0 ? EaseInBounce(time * 2.0, 0.0, newVal, duration) * 0.5 + startVal : EaseOutBounce(time * 2.0 - duration, 0.0, newVal, duration) * 0.5 + newVal * 0.5 + startVal;
        }

        public static double EaseOutExpo(double time, double startVal, double newVal, double duration) => time != duration ? newVal * (-Math.Pow(2.0, -10.0 * time / duration) + 1.0) + startVal : startVal + newVal;

        public static double EaseInExpo(double time, double startVal, double newVal, double duration) => time != 0.0 ? newVal * Math.Pow(2.0, 10.0 * (time / duration - 1.0)) + startVal : startVal;

        public static double EaseInOutExpo(double time, double startVal, double newVal, double duration)
        {
            if (time == 0.0)
                return startVal;
            if (time == duration)
                return startVal + newVal;
            return (time /= duration / 2.0) < 1.0 ? newVal / 2.0 * Math.Pow(2.0, 10.0 * (time - 1.0)) + startVal : newVal / 2.0 * (-Math.Pow(2.0, -10.0 * --time) + 2.0) + startVal;
        }

        public static double EaseOutQuad(double time, double startVal, double newVal, double duration) => -newVal * (time /= duration) * (time - 2.0) + startVal;

        public static double EaseInQuad(double time, double startVal, double newVal, double duration) => newVal * (time /= duration) * time + startVal;

        public static double EaseInOutQuad(double time, double startVal, double newVal, double duration)
        {
            return (time /= duration / 2.0) < 1.0 ? newVal / 2.0 * time * time + startVal : -newVal / 2.0 * (--time * (time - 2.0) - 1.0) + startVal;
        }

        public static double EaseOutSine(double time, double startVal, double newVal, double duration) => newVal * Math.Sin(time / duration * (Math.PI / 2.0)) + startVal;

        public static double EaseInSine(double time, double startVal, double newVal, double duration) => -newVal * Math.Cos(time / duration * (Math.PI / 2.0)) + newVal + startVal;

        public static double EaseInOutSine(double time, double startVal, double newVal, double duration)
        {
            return (time /= duration / 2.0) < 1.0 ? newVal / 2.0 * Math.Sin(Math.PI * time / 2.0) + startVal : -newVal / 2.0 * (Math.Cos(Math.PI * --time / 2.0) - 2.0) + startVal;
        }

        public static double EaseOutCirc(double time, double startVal, double newVal, double duration) => newVal * Math.Sqrt(1.0 - (time = time / duration - 1.0) * time) + startVal;

        public static double EaseInCirc(double time, double startVal, double newVal, double duration) => -newVal * (Math.Sqrt(1.0 - (time /= duration) * time) - 1.0) + startVal;

        public static double EaseInOutCirc(double time, double startVal, double newVal, double duration)
        {
            return (time /= duration / 2.0) < 1.0 ? -newVal / 2.0 * (Math.Sqrt(1.0 - time * time) - 1.0) + startVal : newVal / 2.0 * (Math.Sqrt(1.0 - (time -= 2.0) * time) + 1.0) + startVal;
        }

        public static double EaseOutCubic(double time, double startVal, double newVal, double duration)
        {
            return newVal * ((time = time / duration - 1.0) * time * time + 1.0) + startVal;
        }

        public static double EaseInCubic(double time, double startVal, double newVal, double duration) => newVal * (time /= duration) * time * time + startVal;

        public static double EaseInOutCubic(double time, double startVal, double newVal, double duration)
        {
            return (time /= duration / 2.0) < 1.0 ? newVal / 2.0 * time * time * time + startVal : newVal / 2.0 * ((time -= 2.0) * time * time + 2.0) + startVal;
        }

        public static double EaseOutQuint(double time, double startVal, double newVal, double duration)
        {
            return newVal * ((time = time / duration - 1.0) * time * time * time * time + 1.0) + startVal;
        }

        public static double EaseInQuint(double time, double startVal, double newVal, double duration) => newVal * (time /= duration) * time * time * time * time + startVal;

        public static double EaseInOutQuint(double time, double startVal, double newVal, double duration)
        {
            return (time /= duration / 2.0) < 1.0 ? newVal / 2.0 * time * time * time * time * time + startVal : newVal / 2.0 * ((time -= 2.0) * time * time * time * time + 2.0) + startVal;
        }

        public static double EaseOutBack(double time, double startVal, double newVal, double duration) => newVal * ((time = time / duration - 1.0) * time * (2.70158 * time + 1.70158) + 1.0) + startVal;

        public static double EaseInBack(double time, double startVal, double newVal, double duration) => newVal * (time /= duration) * time * (2.70158 * time - 1.70158) + startVal;

        public static double EaseInOutBack(double time, double startVal, double newVal, double duration)
        {
            double num1 = 1.70158;
            double num2;
            double num3;
            return (time /= duration / 2.0) < 1.0 ? newVal / 2.0 * (time * time * (((num2 = num1 * 1.525) + 1.0) * time - num2)) + startVal : newVal / 2.0 * ((time -= 2.0) * time * (((num3 = num1 * 1.525) + 1.0) * time + num3) + 2.0) + startVal;
        }

        public static double EaseOutQuart(double time, double startVal, double newVal, double duration)
        {
            return -newVal * ((time = time / duration - 1.0) * time * time * time - 1.0) + startVal;
        }

        public static double EaseInQuart(double time, double startVal, double newVal, double duration) => newVal * (time /= duration) * time * time * time + startVal;

        public static double EaseInOutQuart(double time, double startVal, double newVal, double duration)
        {
            return (time /= duration / 2.0) < 1.0 ? newVal / 2.0 * time * time * time * time + startVal : -newVal / 2.0 * ((time -= 2.0) * time * time * time - 2.0) + startVal;
        }

        public static double Linear(double time, double startVal, double newVal, double duration) => newVal * time / duration + startVal;
    }
}
