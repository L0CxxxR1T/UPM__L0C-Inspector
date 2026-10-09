using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 型を選ぶドロップダウンを付ける。対象は2種類。
    /// <list type="bullet">
    /// <item>[SerializeReference] フィールド: 基底型を継承した [Serializable] で引数なしコンストラクタを持つ具象クラスを生成して入れる</item>
    /// <item><see cref="SerializableType"/>（配列・List も可）: 基底型を継承した具象型の名前を保存する。基底型の指定が必須</item>
    /// </list>
    /// </summary>
    /// <remarks>[SerializeReference] の配列・List の要素には未対応（付けると理由を表示して標準の表示に戻る）。</remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TypeFilterAttribute : Attribute
    {
        /// <summary>候補を絞る基底型。null なら [SerializeReference] フィールドの型を使う。</summary>
        public Type BaseType { get; }

        public TypeFilterAttribute(Type baseType = null) => BaseType = baseType;
    }
}
