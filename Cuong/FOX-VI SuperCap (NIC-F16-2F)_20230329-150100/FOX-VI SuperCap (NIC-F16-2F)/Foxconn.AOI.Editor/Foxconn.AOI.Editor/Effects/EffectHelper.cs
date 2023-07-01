using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Foxconn.AOI.Editor.Effects
{
    public static class EffectHelper
    {
        private static readonly PropertyInfo __visualEffectPty = typeof(Visual).GetProperty("VisualEffect", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static Effect GetVisualEffect(this Visual visual)
        {
            return (Effect)__visualEffectPty.GetValue(visual, (object[])null);
        }

        public static void SetVisualEffect(this Visual visual, Effect effect)
        {
            __visualEffectPty.SetValue(visual, effect, (object[])null);
        }
    }
}
