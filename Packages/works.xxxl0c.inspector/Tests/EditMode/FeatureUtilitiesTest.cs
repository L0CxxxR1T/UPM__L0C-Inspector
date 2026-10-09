using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using XXXL0C.Inspector.Editor;

namespace XXXL0C.Inspector.Tests.EditMode
{
  public sealed class EnumToggleOptionsTest
  {
    private enum AliasedKind
    {
      None = 0,
      [InspectorName("主選択")] First = 1,
      FirstAlias = 1,
      Second = 2
    }

    [Flags]
    private enum FlagsKind
    {
      None = 0,
      First = 1,
      Second = 2,
      Combined = First | Second
    }

    [Test]
    public void enumの別名は最初のメンバーだけを表示する()
    {
      List<EnumToggleOption> options = EnumToggleOptions.Build(typeof(AliasedKind));

      Assert.That(options.Count, Is.EqualTo(3));
      Assert.That(options[1].DisplayName, Is.EqualTo("主選択"));
    }

    [Test]
    public void Flagsは単一ビットだけ表示し未表示ビットを保持する()
    {
      List<EnumToggleOption> options = EnumToggleOptions.Build(typeof(FlagsKind));
      List<bool> selection = EnumToggleOptions.ToSelection(options, 5, true);

      Assert.That(options.ConvertAll(option => option.Value), Is.EqualTo(new long[] { 1, 2 }));
      Assert.That(selection, Is.EqualTo(new[] { true, false }));
      Assert.That(EnumToggleOptions.FromSelection(options, new[] { false, true }, 5, true), Is.EqualTo(6));
    }
  }

  public sealed class NumericBoundsUtilityTest
  {
    private ConditionTestAsset _asset;
    private SerializedObject _serializedObject;

    [SetUp]
    public void SetUp()
    {
      _asset = ScriptableObject.CreateInstance<ConditionTestAsset>();
      _serializedObject = new SerializedObject(_asset);
    }

    [TearDown]
    public void TearDown()
    {
      UnityEngine.Object.DestroyImmediate(_asset);
    }

    [Test]
    public void 整数の下限は切り上げて丸める()
    {
      SerializedProperty property = _serializedObject.FindProperty("_count");
      property.longValue = 1;

      Assert.That(NumericBoundsUtility.TryFindViolation(property, 1.5, true, out string actual), Is.True);
      Assert.That(actual, Is.EqualTo("1"));
      Assert.That(NumericBoundsUtility.Clamp(property, 1.5, true), Is.True);
      Assert.That(property.longValue, Is.EqualTo(2));
    }

    [Test]
    public void doubleの境界値は保存精度で比較する()
    {
      SerializedProperty property = _serializedObject.FindProperty("_preciseRatio");
      property.doubleValue = 0.1;

      Assert.That(NumericBoundsUtility.TryFindViolation(property, 0.1, true, out _), Is.False);
    }
  }

  public sealed class ValueDropdownSourceTest
  {
    private static readonly ValueDropdownList<int> CANDIDATES = new ValueDropdownList<int>
        {
            { "少ない", 2 },
            { "多い", 5 }
        };

    [Test]
    public void staticReadonly候補の表示名と値を読み取る()
    {
      bool found = ValueDropdownSource.TryGetEntries(
          typeof(ValueDropdownSourceTest), typeof(int), new ValueDropdownAttribute(nameof(CANDIDATES)),
          out List<ValueDropdownEntry> entries, out string reason);

      Assert.That(found, Is.True, reason);
      Assert.That(entries.Count, Is.EqualTo(2));
      Assert.That(entries[0].Text, Is.EqualTo("少ない"));
      Assert.That(entries[1].Value, Is.EqualTo(5));
    }
  }

  public sealed class GroupTreeTest
  {
    [Test]
    public void TabGroupのパスからページを解決する()
    {
      VisualElement rootContent = new VisualElement();
      GroupTree tree = new GroupTree();
      GroupNode root = tree.CreateRoot("root", rootContent);
      TabGroupFactory factory = new TabGroupFactory();

      GroupNode basicPage = tree.Resolve(root, "詳細/基本", factory);
      GroupNode extraPage = tree.Resolve(root, "詳細/補足", factory);

      Assert.That(basicPage.IsPage, Is.True);
      Assert.That(extraPage.IsPage, Is.True);
      Assert.That(basicPage.FactoryType, Is.EqualTo(typeof(TabGroupFactory)));
      Assert.That(extraPage.Key, Is.EqualTo("root/詳細/補足"));
      Assert.That(rootContent.childCount, Is.EqualTo(1));
    }
  }
}