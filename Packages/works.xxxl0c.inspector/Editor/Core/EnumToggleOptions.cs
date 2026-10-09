using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[EnumToggleButtons] のボタン1つ分。</summary>
    internal readonly struct EnumToggleOption
    {
        public string DisplayName { get; }
        public long Value { get; }

        public EnumToggleOption(string displayName, long value)
        {
            DisplayName = displayName;
            Value = value;
        }
    }

    /// <summary>
    /// [EnumToggleButtons] のボタンの並びと、enum の値 ⇔ ボタンの選択状態の変換。UI に依存しない。
    /// </summary>
    internal static class EnumToggleOptions
    {
        /// <summary>
        /// ボタンにする値を宣言順に並べる。同じ値の別名は最初の名前だけを使う。
        /// [Flags] なら 0 と、複数ビットを合わせた値はボタンにしない（個々のビットの組み合わせで表せるため）。
        /// </summary>
        public static List<EnumToggleOption> Build(Type enumType)
        {
            bool isFlags = IsFlags(enumType);
            List<EnumToggleOption> options = new List<EnumToggleOption>();
            HashSet<long> seen = new HashSet<long>();

            foreach (FieldInfo member in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                long value = Convert.ToInt64(member.GetValue(null));
                if (isFlags && !IsSingleBit(value)) continue;
                if (!seen.Add(value)) continue;

                InspectorNameAttribute inspectorName = member.GetCustomAttribute<InspectorNameAttribute>();
                string displayName = inspectorName != null
                    ? inspectorName.displayName
                    : ObjectNames.NicifyVariableName(member.Name);

                options.Add(new EnumToggleOption(displayName, value));
            }

            return options;
        }

        public static bool IsFlags(Type enumType) => enumType.IsDefined(typeof(FlagsAttribute), false);

        public static List<bool> ToSelection(List<EnumToggleOption> options, long value, bool isFlags)
        {
            List<bool> selection = new List<bool>(options.Count);
            foreach (EnumToggleOption option in options)
            {
                selection.Add(isFlags ? (value & option.Value) == option.Value : value == option.Value);
            }

            return selection;
        }

        /// <summary>
        /// 選択状態を enum の値に戻す。[Flags] でボタンに出していないビット（名前の無いビット）は current のまま残す。
        /// 単一選択で何も選ばれていなければ current を返す。
        /// </summary>
        public static long FromSelection(List<EnumToggleOption> options, IReadOnlyList<bool> selection, long current, bool isFlags)
        {
            if (!isFlags)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (selection[i]) return options[i].Value;
                }

                return current;
            }

            long shownBits = 0;
            long selectedBits = 0;
            for (int i = 0; i < options.Count; i++)
            {
                shownBits |= options[i].Value;
                if (selection[i]) selectedBits |= options[i].Value;
            }

            return (current & ~shownBits) | selectedBits;
        }

        private static bool IsSingleBit(long value) => value != 0 && (value & (value - 1)) == 0;
    }
}
