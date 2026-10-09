using System;

namespace XXXL0C.Inspector.Editor
{
    internal static class GenericTypeUtility
    {
        /// <summary>
        /// type の中から definition を閉じたジェネリック型を探す。配列・List・Dictionary の中にあっても見つける。
        /// PropertyDrawer.fieldInfo はコレクションの要素でもコレクション側のフィールドを指すため。
        /// </summary>
        public static Type FindClosed(Type type, Type definition)
        {
            if (type == null) return null;
            if (type.IsArray) return FindClosed(type.GetElementType(), definition);
            if (!type.IsGenericType) return null;
            if (type.GetGenericTypeDefinition() == definition) return type;

            foreach (Type argument in type.GetGenericArguments())
            {
                Type found = FindClosed(argument, definition);
                if (found != null) return found;
            }

            return null;
        }
    }
}
