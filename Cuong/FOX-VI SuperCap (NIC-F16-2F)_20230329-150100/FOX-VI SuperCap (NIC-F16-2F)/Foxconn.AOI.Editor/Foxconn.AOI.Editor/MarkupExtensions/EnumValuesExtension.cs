using System;
using System.Windows.Markup;

namespace Foxconn.AOI.Editor.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(Array))]
    public sealed class EnumValuesExtension : MarkupExtension
    {
        private string _typeName;
        private Type _type;

        public EnumValuesExtension()
        {
        }

        public EnumValuesExtension(string typeName) => _typeName = typeName;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_type == null)
                _type = (Type)new TypeExtension(_typeName).ProvideValue(serviceProvider);
            return Enum.GetValues(_type);
        }
    }
}
