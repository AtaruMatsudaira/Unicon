# Unity Dock Icon Changer for macOS

複数のUnityエディタを並列で起動した際に、macOSのDock上で異なるアイコンを表示することで、プロジェクトの識別を容易にするエディタ拡張です。

## 機能

- **カスタム画像アイコン**: 任意の画像ファイルをDockアイコンとして使用
- **カラーオーバーレイ**: Unityアイコンに色を重ねて表示
- **自動カラー生成**: プロジェクト名から自動的に一意の色を生成
- **自動適用**: エディタ起動時に自動的にアイコンを変更
- **Preferences UI**: Edit > Preferences から簡単に設定

## 動作環境

- **プラットフォーム**: macOS 10.13以降
- **Unity**: 2020.3以降
- **アーキテクチャ**: x86_64, arm64 (Apple Silicon)

## インストール

このプロジェクトには既に実装済みです。以下のファイルが含まれています:

```
mac_icon_changer/
├── Assets/
│   ├── Editor/
│   │   └── DockIconChanger/
│   │       ├── DockIconSettings.cs
│   │       ├── DockIconInitializer.cs
│   │       ├── DockIconPreferences.cs
│   │       └── NativeMethods.cs
│   └── Plugins/
│       └── Editor/
│           └── macOS/
│               └── DockIconPlugin.bundle/
└── DockIconPlugin/              # Xcodeプロジェクト（開発用）
    ├── DockIconPlugin.xcodeproj
    └── DockIconPlugin/
        ├── DockIconPlugin.swift
        └── Info.plist
```

## 使い方

### 1. 自動適用（デフォルト）

Unityエディタを起動すると、自動的にプロジェクト名から生成された色のオーバーレイが適用されます。

### 2. カスタム画像を使用

1. Unity Editorで `Edit > Preferences > Dock Icon Changer` を開く
2. "Browse" ボタンをクリック
3. 任意の画像ファイル（PNG, JPG, ICNS等）を選択
4. Dockアイコンが即座に変更されます

### 3. カラーオーバーレイをカスタマイズ

1. `Edit > Preferences > Dock Icon Changer` を開く
2. "Use Auto Color" のチェックを外す
3. "Overlay Color" で好きな色を選択
4. "Apply Current Settings" をクリック

### 4. デフォルトに戻す

`Edit > Preferences > Dock Icon Changer` で "Reset to Default" をクリックすると、Unity標準アイコンに戻ります。

## 設定の保存

設定は `UserSettings/DockIconSettings.json` に保存されます。このファイルは自動的に `.gitignore` の対象となり、個人設定として管理されます。

## 開発者向け情報

### プラグインのビルド

ネイティブプラグインを再ビルドする場合:

```bash
cd DockIconPlugin
xcodebuild -project DockIconPlugin.xcodeproj \
  -scheme DockIconPlugin \
  -configuration Release \
  -arch x86_64 -arch arm64 \
  ONLY_ACTIVE_ARCH=NO \
  BUILD_DIR=./build \
  clean build

# ビルドされたバンドルをUnityプロジェクトにコピー
cp -r build/Release/DockIconPlugin.bundle ../mac_icon_changer/Assets/Plugins/Editor/macOS/
```

### アーキテクチャ

#### Swift実装 (DockIconPlugin.swift)

macOSネイティブAPI `NSApplication.applicationIconImage` を使用してDockアイコンを変更します。

主要な関数:
- `SetDockIconFromPath`: 画像ファイルパスからアイコンを設定
- `SetDockIconWithColorOverlay`: カラーオーバーレイでアイコンを設定
- `ResetDockIcon`: デフォルトアイコンに戻す（nil設定）

#### C# 実装

- **DockIconSettings**: 設定の保存・読み込み管理
- **NativeMethods**: P/Invokeによるネイティブプラグイン呼び出し
- **DockIconInitializer**: `[InitializeOnLoad]`でエディタ起動時に自動適用
- **DockIconPreferences**: `[SettingsProvider]`でPreferences UI提供

## トラブルシューティング

### プラグインが読み込まれない

1. `Assets/Plugins/Editor/macOS/DockIconPlugin.bundle` が存在することを確認
2. Unityを再起動
3. Consoleでエラーメッセージを確認

### アイコンが変更されない

1. macOS専用機能です（Windows/Linuxでは動作しません）
2. `Edit > Preferences > Dock Icon Changer` で設定を確認
3. "Apply Current Settings" ボタンをクリック

### 画像が読み込まれない

- 絶対パスが必要です（相対パスは使用できません）
- NSImageが対応するフォーマット（PNG, JPG, ICNS等）を使用してください

## ライセンス

このプロジェクトは参考実装です。自由に使用・改変してください。

## 参考資料

- [NSApplication.applicationIconImage - Apple Developer](https://developer.apple.com/documentation/appkit/nsapplication/1428744-applicationiconimage)
- [Unity Native Plugins](https://docs.unity3d.com/Manual/NativePlugins.html)
- [InitializeOnLoadAttribute](https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html)
- [SettingsProvider](https://docs.unity3d.com/ScriptReference/SettingsProvider.html)
