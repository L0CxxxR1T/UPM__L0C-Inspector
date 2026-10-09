using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[ReadOnly] の装飾。表示のみにする。折りたたみの開閉はできる。</summary>
    public sealed class ReadOnlyDecorator : IPropertyDecorator
    {
        public Type AttributeType => typeof(ReadOnlyAttribute);

        public int Order => 0;

        public void Decorate(DecorationContext context) => ReadOnlyGuard.SetLocked(context.Field, AttributeType, true);
    }
}
