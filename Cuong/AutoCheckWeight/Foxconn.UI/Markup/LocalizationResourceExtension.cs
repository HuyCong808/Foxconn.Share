using System;
using System.Windows;
using System.Windows.Markup;

namespace Foxconn.UI.Markup
{
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class LocalizationResourceExtension : DynamicResourceExtension
    {
        [ConstructorArgument("localizationKey")]
        public object LocalizationKey
        {
            get => !(ResourceKey is LocalizationResourceKey resourceKey) ? null : resourceKey.InternalKey;
            set => ResourceKey = value != null ? (object)new LocalizationResourceKey(value) : throw new ArgumentNullException(nameof(value));
        }

        public LocalizationResourceExtension()
        {
        }

        public LocalizationResourceExtension(object localizationKey) => LocalizationKey = localizationKey != null ? localizationKey : throw new ArgumentNullException(nameof(localizationKey));
    }
}
