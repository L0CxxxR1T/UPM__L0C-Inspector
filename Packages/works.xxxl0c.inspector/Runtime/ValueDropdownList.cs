using System.Collections.Generic;

namespace XXXL0C.Inspector
{
    /// <summary>[ValueDropdown] の候補を表示名付きで並べるリスト。コレクション初期化子で書ける。</summary>
    public sealed class ValueDropdownList<T> : List<ValueDropdownItem<T>>
    {
        public void Add(string text, T value) => Add(new ValueDropdownItem<T>(text, value));
    }
}
