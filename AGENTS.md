# AGENTS.md

このリポジトリで作業するエージェント（Claude Code / Codex）向けのガイド。`CLAUDE.md` はこのファイルを読み込むだけなので、編集はこのファイルに対して行う。

## プロジェクト概要

自前のインスペクタ拡張（属性・型・シリアライズ拡張）ライブラリの開発用プロジェクト。

- 本体: Embedded パッケージ `Packages/works.xxxl0c.inspector/`
- サンプル: `Assets/_Samples/XXXL0C.Inspector/`
- ドキュメント: パッケージ側の `Packages/works.xxxl0c.inspector/README.md` が正。ルートの `README.md` は導入案内だけ
- 属性・Drawer・Editor の設計を決めるときは `unity-coding-rules` スキルの `references/editor-extension.md` に従う

## 属性・機能を足すときの完了条件

次の4つが揃って完了。

1. Runtime 側の属性クラス
2. Editor 側の実装（`ExtensionRegistry` が自動収集するので登録処理は不要）
3. サンプル: 確認用の `*SampleBehaviour` に使用例を足し、クラスの `<summary>` に期待する挙動を「〜こと」で書く
4. パッケージ README の該当節

## Unity バージョン

- 対応下限は `package.json` の `unity`（6000.3）、開発エディタは 6000.6。エディタでコンパイルが通っても下限で通る保証は無い
- 6.6 以降の API は `#if UNITY_6000_6_OR_NEWER` で囲む（例: `Editor/Core/DictionaryUtility.cs`）

## Unity 開発の前提

C#/Unity の規約と Editor 操作方針（コンパイル確認・テスト・Play確認）は `unity-coding-rules` スキルが正。ここにはスキルが参照するプロジェクト固有の事実だけを書く。

- DI: なし（ライブラリのため）
- 公式 Unity CLI の pipeline（`com.unity.pipeline`）: 導入済み
- uloop（`io.github.hatayama.uloopmcp`）: 導入済み
- テスト asmdef: パッケージ内の `Packages/works.xxxl0c.inspector/Tests/EditMode/XXXL0C.Inspector.Tests.EditMode.asmdef`。Runtime / Editor とも `AssemblyInfo.cs` で internal を公開済み
