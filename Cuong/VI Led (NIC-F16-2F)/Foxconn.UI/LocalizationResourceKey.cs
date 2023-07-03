namespace Foxconn.UI
{
    internal sealed class LocalizationResourceKey
    {
        public object InternalKey { get; private set; }

        public LocalizationResourceKey(object key) => InternalKey = key;

        public override bool Equals(object obj) => obj is LocalizationResourceKey && InternalKey.Equals(((LocalizationResourceKey)obj).InternalKey);

        public override int GetHashCode() => InternalKey.GetHashCode() ^ typeof(LocalizationResourceKey).GetHashCode();
    }
}
