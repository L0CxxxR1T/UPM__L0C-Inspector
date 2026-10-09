# AGENTS.md

このリポジトリで作業するエージェント（Claude Code / Codex）向けのガイド。`CLAUDE.md` はこのファイルを読み込むだけなので、編集はこのファイルに対して行う。

## プロジェクト概要

自前のインスペクタ拡張（属性・型・シリアライズ拡張）ライブラリの開発用プロジェクト。

- 本体: Embedded パッケージ `Packages/works.xxxl0c.inspector/`
- サンプル: `Assets/_Samples/XXXL0C.Inspector/`
- 属性・Drawer・Editor の設計を決めるときは `unity-coding-rules` スキルの `references/editor-extension.md` に従う

## Unity 開発の前提

C#/Unity の規約と Editor 操作方針（コンパイル確認・テスト・Play確認）は `unity-coding-rules` スキルが正。ここにはスキルが参照するプロジェクト固有の事実だけを書く。

- DI: なし（ライブラリのため）
- 公式 Unity CLI の pipeline（`com.unity.pipeline`）: 導入済み
- uloop（`io.github.hatayama.uloopmcp`）: 導入済み
- テスト asmdef: `Packages/works.xxxl0c.inspector/Tests/EditMode/XXXL0C.Inspector.Tests.EditMode.asmdef`（Editor 側の internal は `Editor/AssemblyInfo.cs` で公開）
