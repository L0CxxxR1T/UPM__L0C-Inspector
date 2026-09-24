# AGENTS.md

このリポジトリで作業するエージェント（Claude Code / Codex）向けのガイド。`CLAUDE.md` はこのファイルを読み込むだけなので、編集はこのファイルに対して行う。

## プロジェクト概要

自前のインスペクタ拡張（属性・型・シリアライズ拡張）ライブラリの開発用プロジェクト。本体は Embedded パッケージ `Packages/com.xxxl0c.inspector/` で、設計方針は `unity-coding-rules` スキルの `references/editor-extension.md` が正。

## Unity 開発の前提

C#/Unity のコーディング規約と Editor 操作方針（コンパイル確認・テスト・Play確認）は `unity-coding-rules` スキルに従う。このファイルには重複して書かない。

- DI: なし（ライブラリのため）
- 公式 Unity CLI の pipeline（`com.unity.pipeline`）: 導入済み（`0.7.0-exp.1`）。uloop（`io.github.hatayama.uloopmcp`）は移行期間中の補完用
- asmdef: `XXXL0C.Inspector`（Runtime） / `XXXL0C.Inspector.Editor`（Editor） / サンプルは `Assets/_Samples/XXXL0C.Inspector/` / テスト asmdef: まだ無い
