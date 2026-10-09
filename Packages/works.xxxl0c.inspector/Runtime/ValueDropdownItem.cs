namespace XXXL0C.Inspector
{
    /// <summary>[ValueDropdown] の候補1件。</summary>
    public readonly struct ValueDropdownItem<T> : IValueDropdownItem
    {
        public string Text { get; }
        public T Value { get; }

        object IValueDropdownItem.Value => Value;

        public ValueDropdownItem(string text, T value)
        {
            Text = text;
            Value = value;
        }
    }
}
