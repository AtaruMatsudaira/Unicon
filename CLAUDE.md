# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## このリポジトリの性質

これは Unity Editor のドック / タスクバーアイコンを macOS・Windows でカスタマイズする UPM パッケージ **`com.mattun.unicon`（Unicon）** の開発リポジトリ。リポジトリ自身は Unity プロジェクトではない。配布物は `Packages/com.mattun.unicon/` 配下のみで、ルートの `README.md` / `README_ja.md` はそこへのシンボリックリンク。

ディレクトリの役割が層になっている点が最重要：

- `Packages/com.mattun.unicon/` … 配布される UPM パッケージ本体（C# + ビルド済みネイティブバイナリ）
- `Plugins/`（ルート直下、**パッケージには含まれない**） … ネイティブプラグインの**ソースとビルドスクリプト**
- `Sample/` … 動作確認用の Unity プロジェクト（後述）。`file:` 参照でパッケージを埋め込む

## アーキテクチャ（複数ファイルを跨ぐ全体像）

アイコン変更は「C# → プラットフォーム振り分け → P/Invoke → ネイティブ」の一方向フロー。

1. **適用トリガー** … `UniconInitializer.cs`（`[InitializeOnLoad]` / `[DidReloadScripts]`）。**macOS は `EditorApplication.update` で定期的に再適用する**（OS がアイコンを勝手に戻すため）。Windows は `delayCall` で一度だけ。
2. **設定** … `UniconSettings.cs`。`UserSettings/DockIconSettings.json` に保存（VC 対象外）。`UseAutoColor` 時はプロジェクト名のハッシュから色を生成。
3. **UI** … `UniconPreferences.cs`（`[SettingsProvider]`、`Edit > Preferences > Unicon`）。
4. **振り分け** … `NativeMethods.cs` がコンパイル時 `#if UNITY_EDITOR_OSX / WIN / else` で `INativeMethods` 実装を 1 つ選ぶ（`Internal/` に Mac / Windows / Dummy の 3 実装）。
5. **ネイティブ境界** … 統一 API は実質 2 つだけ：`SetIconUnified(imagePath, overlayColor, text, textColor, fontSizeMultiplier)` と `ResetIcon()`。

**プラットフォームで合成の場所が違う点に注意：**

- **macOS**：画像読み込み・カラーオーバーレイ・バッジ文字の描画をすべて Swift 側（`UniconPlugin.swift`）で行い、`NSApplication.applicationIconImage` に設定する。
- **Windows**：合成を **C# 側で** 行う。`Native/WindowsBitmapModifier.cs`（`unsafe` でピクセル操作）でオーバーレイ色とバッジ文字を `Bitmap` に焼き込み、生成した HICON を DLL に渡す。DLL は Windows Shell API（プロセスごとの AppUserModelID）でタスクバーアイコンを差し替えるだけ。`fontSizeMultiplier` は Windows では無視される。

### asmdef / 可視性の制約

- アセンブリは 2 つ：`Unicon.Editor`（メイン）と `Unicon.Editor.Native`（`allowUnsafeCode: true`、`WindowsBitmapModifier` 用）。メイン側は unsafe 不可なので、ピクセル操作コードは必ず Native アセンブリ側に置く。
- C# 型は原則 `internal`。例外は `public static class WindowsBitmapModifier` のみ。

## ネイティブプラグインのビルド

`Packages/.../Plugins/Editor/{macOS,Windows}/` のバイナリは**ビルド済みでコミットされている**。`Plugins/`（ルート）のソースを変更したら、必ず再ビルド＆再配置しないと反映されない。

### macOS（Swift / Universal Binary）

```bash
./Plugins/macOS/build_and_deploy.sh
```

xcodebuild で arm64 + x86_64 をビルドし、`UniconPlugin.bundle` をパッケージへ自動コピーし、必須シンボル（`SetDockIconUnified` / `ResetDockIcon`）の存在まで検証する。実行後は Unity Editor の再起動が必要。

### Windows（C++ / CMake）

```bash
cd Plugins/Windows/Unicon
mkdir build && cd build
cmake ..
cmake --build . --config Release
```

出力 `UniconPluginForWindows.dll` を `Packages/com.mattun.unicon/Plugins/Editor/Windows/` へ手動コピー（macOS と違い自動配置スクリプトはない）。

## 動作確認・テスト

ユニットテストは存在しない。動作確認は `Sample/`（Unity **6000.2.9f1**）を Unity で開き、Preferences UI を操作して行う。

Editor 自動操作用に **uLoopMCP** が `.mcp.json` で設定済み（TCP ポート 8701、サーバ実体は `Sample/Library/PackageCache/` 内）。コンパイル確認・ログ取得・スクリーンショット・Play Mode 制御などは `uloop-*` スキル群を使う。

## リリース

GitHub Actions `release.yml`（`workflow_dispatch`、`version` を手入力）で完結する。`package.json` のバージョンを書き換え → コミット → `vX.Y.Z` タグ → GitHub Release を作成。手動でバージョンを上げる必要はない。配布は OpenUPM と Git URL。

## 注意点

- 旧名 **DockIconChanger** の名残がある（`Sample/` の一部 csproj 名、空の `Editor/DockIconChanger/` ディレクトリ、`.gitignore` の `mac_icon_changer/` 等の古いパス）。現行の namespace・識別子はすべて `Unicon`。
- ルートの `README.md` / `README_ja.md` はシンボリックリンク。ドキュメント本体は `Packages/com.mattun.unicon/` 側を編集する。
