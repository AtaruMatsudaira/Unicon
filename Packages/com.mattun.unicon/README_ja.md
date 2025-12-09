# Unicon

[English](README.md) | 日本語

macOSとWindows上でUnity EditorのDock/タスクバーアイコンをカスタマイズできます。並行して実行している複数のUnityインスタンスを簡単に見分けられます。

![Unity Version](https://img.shields.io/badge/unity-2020.3%2B-blue)
![License](https://img.shields.io/badge/license-MIT-green)

![画面収録 2025-12-09 22 04 26](https://github.com/user-attachments/assets/798ae8d0-34bd-43c8-9cc3-ef0beae3aaac)

## 機能

- **カスタム画像アイコン**: 任意の画像ファイルをDockアイコンとして使用
- **カラーオーバーレイ**: Unityアイコンに色を重ねる
- **自動カラー生成**: プロジェクト名から一意の色を自動生成
- **自動適用**: エディタ起動時に自動的にアイコンを適用
- **環境設定UI**: Edit > Preferencesから簡単に設定

## 動作環境

- **プラットフォーム**:
  - macOS 10.13以降
  - Windows 7以降
- **Unity**: 2020.3以降
- **アーキテクチャ**:
  - macOS: x86_64, arm64 (Apple Silicon)
  - Windows: x86_64

## インストール

### OpenUPM経由（推奨）

```bash
openupm add com.mattun.unicon
```

または、`Packages/manifest.json` に scoped registry を手動で追加:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.mattun.unicon"
      ]
    }
  ],
  "dependencies": {
    "com.mattun.unicon": "1.2.4"
  }
}
```

### Git URL経由 (Unity Package Manager)

1. **Window > Package Manager** を開く
2. **+** ボタンをクリックして **Add package from git URL...** を選択
3. 以下を入力: `https://github.com/AtaruMatsudaira/Unicon.git?path=Packages/com.mattun.unicon`

### manifest.json経由

`Packages/manifest.json` に以下を追加:

```json
{
  "dependencies": {
    "com.mattun.unicon": "https://github.com/AtaruMatsudaira/Unicon.git?path=Packages/com.mattun.unicon"
  }
}
```

## 使い方

### クイックスタート

1. **Edit > Preferences > Unicon** を開く
2. **"Enable Custom Dock Icon"** トグルを有効化
3. 以下のいずれかを選択:
   - **カスタム画像**: "Browse"ボタンをクリックして画像ファイルを選択
   - **カラーオーバーレイ**: "Use Auto Color"を無効にして色を選択

### 自動カラー

デフォルトでは、パッケージがプロジェクト名に基づいて一意の色を生成します。カスタムアイコンを設定せずにプロジェクトを素早く見分けたい場合に便利です。

### 設定

設定は `UserSettings/DockIconSettings.json` に保存され、自動的にバージョン管理から除外されます。

## 仕組み

このパッケージは、ネイティブプラグインを使用してランタイムでDock/タスクバーアイコンを変更します：

- **macOS**: `DockIconPlugin.bundle`（Swift）を使用し、`NSApplication.applicationIconImage` APIを活用
- **Windows**: `DockIconPluginForWindows.dll`（C++）を使用し、Windows Shell APIを活用してタスクバーアイコンを変更

### アーキテクチャ

#### macOS
```
┌─────────────────┐
│  Unity Editor   │
│   (C# Scripts)  │
└────────┬────────┘
         │ P/Invoke
         ↓
┌─────────────────┐
│ DockIconPlugin  │
│    (Swift)      │
└────────┬────────┘
         │ NSApplication API
         ↓
┌─────────────────┐
│   macOS Dock    │
└─────────────────┘
```

#### Windows
```
┌──────────────────────┐
│   Unity Editor       │
│    (C# Scripts)      │
└──────────┬───────────┘
           │ P/Invoke
           ↓
┌──────────────────────┐
│ DockIconPluginFor    │
│      Windows (C++)   │
└──────────┬───────────┘
           │ Windows Shell API
           ↓
┌──────────────────────┐
│  Windows Taskbar     │
└──────────────────────┘
```

## APIリファレンス

### DockIconSettings

```csharp
// カスタムDockアイコンの有効/無効
DockIconSettings.Enabled = true;

// カスタム画像パスの設定
DockIconSettings.IconPath = "/path/to/icon.png";

// 自動カラー生成の有効化
DockIconSettings.UseAutoColor = true;

// カスタムオーバーレイカラーの設定
DockIconSettings.OverlayColor = new Color(1f, 0.5f, 0f, 0.3f);

// 設定の保存
DockIconSettings.Save();
```

### NativeMethods

```csharp
// ファイルパスからアイコンを設定
NativeMethods.SetIconFromPath("/path/to/icon.png");

// カラーオーバーレイでアイコンを設定
NativeMethods.SetIconWithColorOverlay(new Color(1f, 0.5f, 0f, 0.3f));

// デフォルトアイコンにリセット
NativeMethods.ResetIcon();
```

## トラブルシューティング

### プラグインが読み込まれない

#### macOS
1. `DockIconPlugin.bundle` が `Packages/com.mattun.unicon/Plugins/Editor/macOS/` に存在するか確認
2. Unity Editorを再起動
3. Consoleでエラーメッセージを確認

#### Windows
1. `DockIconPluginForWindows.dll` が `Packages/com.mattun.unicon/Plugins/Editor/Windows/` に存在するか確認
2. Unity Editorを再起動
3. Consoleでエラーメッセージを確認

### アイコンが変更されない

1. この機能は現在macOSとWindowsでのみサポートされています（Linuxは非対応）
2. "Enable Custom Dock Icon"がONになっているか確認
3. Preferencesで"Apply Current Settings"ボタンをクリック

### 画像が読み込まれない

- 絶対パスのみサポート（相対パスは動作しません）
- NSImage互換フォーマットを使用: PNG、JPG、ICNSなど

## プラグインのビルド

ネイティブプラグインを再ビルドする必要がある場合:

### macOS用プラグイン

```bash
cd path/to/Plugins/macOS/DockIconPlugin
xcodebuild -project DockIconPlugin.xcodeproj \
  -scheme DockIconPlugin \
  -configuration Release \
  -arch x86_64 -arch arm64 \
  ONLY_ACTIVE_ARCH=NO \
  BUILD_DIR=./build \
  clean build

# パッケージにコピー
cp -r build/Release/DockIconPlugin.bundle \
  path/to/Packages/com.mattun.unicon/Plugins/Editor/macOS/
```

### Windows用プラグイン

```bash
cd path/to/Plugins/Windows/DockIconPlugin
mkdir build && cd build
cmake ..
cmake --build . --config Release

# パッケージにコピー
cp Release/DockIconPluginForWindows.dll \
  path/to/Packages/com.mattun.unicon/Plugins/Editor/Windows/
```

## ライセンス

MIT License

Copyright (c) 2025 mattun

## 参考資料

- [NSApplication.applicationIconImage - Apple Developer](https://developer.apple.com/documentation/appkit/nsapplication/1428744-applicationiconimage)
- [Unity Native Plugins](https://docs.unity3d.com/Manual/NativePlugins.html)
- [InitializeOnLoadAttribute](https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html)
- [SettingsProvider](https://docs.unity3d.com/ScriptReference/SettingsProvider.html)
