namespace XXXL0C.Inspector
{
    /// <summary>[ValueDropdown] の候補1件。表示名と値の組。</summary>
    public interface IValueDropdownItem
    {
        string Text { get; }
        object Value { get; }
    }
}
