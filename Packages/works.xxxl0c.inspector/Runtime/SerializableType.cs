using System;
using UnityEngine;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// インスペクタで選んだ型を保存する。保存するのは型の完全名（タグ）だけで、実行時に Type へ戻すときは
    /// <see cref="SerializableTypeRegistry"/> に明示的に登録した型からしか引かない。
    /// </summary>
    /// <remarks>
    /// 名前からリフレクションで型を解決しないのは、IL2CPP のコードストリッピングで型が消えると
    /// ビルドでだけ黙って壊れるため。登録するコードが型を参照するので、登録した型はストリップされない。
    /// 候補の絞り込みは [TypeFilter(typeof(基底型))] で行う。
    /// </remarks>
    [Serializable]
    public struct SerializableType : IEquatable<SerializableType>
    {
        [SerializeField] private string _typeName;

        /// <summary>保存している型の完全名（Type.FullName）。未設定なら空文字。</summary>
        public string TypeName => _typeName ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(_typeName);

        public SerializableType(Type type) => _typeName = type?.FullName;

        /// <summary>登録済みの型に戻す。未設定または未登録なら false。</summary>
        public bool TryResolve(out Type type) => SerializableTypeRegistry.TryGet(TypeName, out type);

        /// <summary>登録済みの型に戻す。未設定・未登録はバグなので例外を投げる。</summary>
        public Type Resolve()
        {
            if (IsEmpty) throw new InvalidOperationException("SerializableType が未設定です。");
            if (TryResolve(out Type type)) return type;

            throw new InvalidOperationException(
                $"型 '{_typeName}' は SerializableTypeRegistry に登録されていません。起動時に Register してください。");
        }

        public bool Equals(SerializableType other) => string.Equals(TypeName, other.TypeName, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is SerializableType other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(TypeName);

        public override string ToString() => IsEmpty ? "（なし）" : _typeName;
    }
}
