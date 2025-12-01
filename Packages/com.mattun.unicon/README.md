# Unicon

English | [日本語](https://github.com/AtaruMatsudaira/Unicon/blob/main/Packages/com.mattun.unicon/README_ja.md)

Customize Unity Editor dock/taskbar icon on macOS and Windows. Easily distinguish between multiple Unity instances running in parallel.

![Unity Version](https://img.shields.io/badge/unity-2020.3%2B-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

- **Custom Image Icon**: Use any image file as your dock icon
- **Color Overlay**: Apply a color overlay to the Unity icon
- **Auto Color Generation**: Automatically generate a unique color from project name
- **Auto Apply**: Automatically applies icon on editor startup and script reload
- **Preferences UI**: Easy configuration via Edit > Preferences

## Requirements

- **Platform**:
  - macOS 10.13 or later
  - Windows 7 or later
- **Unity**: 2020.3 or later
- **Architecture**:
  - macOS: x86_64, arm64 (Apple Silicon)
  - Windows: x86_64

## Installation

### Via Git URL (Unity Package Manager)

1. Open **Window > Package Manager**
2. Click **+** button and select **Add package from git URL...**
3. Enter: `https://github.com/mattun/Unicon.git?path=Packages/com.mattun.unicon`

### Via manifest.json

Add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.mattun.unicon": "https://github.com/mattun/Unicon.git?path=Packages/com.mattun.unicon"
  }
}
```

## Usage

### Quick Start

1. Open **Edit > Preferences > Dock Icon Changer**
2. Enable **"Enable Custom Dock Icon"** toggle
3. Choose one of the following:
   - **Custom Image**: Click "Browse" to select an image file
   - **Color Overlay**: Disable "Use Auto Color" and pick a color

### Auto Color

By default, the package generates a unique color based on your project name. This is useful when you want to quickly distinguish between projects without setting custom icons.

### Settings

Settings are saved in `UserSettings/DockIconSettings.json` and automatically excluded from version control.

## How It Works

This package uses native plugins to change the dock/taskbar icon at runtime:

- **macOS**: Uses `DockIconPlugin.bundle` (Swift) that leverages `NSApplication.applicationIconImage` API
- **Windows**: Uses `DockIconPluginForWindows.dll` (C++) that leverages Windows Shell API to change the taskbar icon

### Architecture

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

## API Reference

### DockIconSettings

```csharp
// Enable/disable custom dock icon
DockIconSettings.Enabled = true;

// Set custom image path
DockIconSettings.IconPath = "/path/to/icon.png";

// Enable auto color generation
DockIconSettings.UseAutoColor = true;

// Set custom overlay color
DockIconSettings.OverlayColor = new Color(1f, 0.5f, 0f, 0.3f);

// Save settings
DockIconSettings.Save();
```

### NativeMethods

```csharp
// Set icon from file path
NativeMethods.SetIconFromPath("/path/to/icon.png");

// Set icon with color overlay
NativeMethods.SetIconWithColorOverlay(new Color(1f, 0.5f, 0f, 0.3f));

// Reset to default icon
NativeMethods.ResetIcon();
```

## Troubleshooting

### Plugin not loading

#### macOS
1. Check if `DockIconPlugin.bundle` exists in `Packages/com.mattun.unicon/Plugins/Editor/macOS/`
2. Restart Unity Editor
3. Check Console for error messages

#### Windows
1. Check if `DockIconPluginForWindows.dll` exists in `Packages/com.mattun.unicon/Plugins/Editor/Windows/`
2. Restart Unity Editor
3. Check Console for error messages

### Icon not changing

1. This feature is currently supported on macOS and Windows only (Linux not supported)
2. Make sure "Enable Custom Dock Icon" is toggled ON
3. Click "Apply Current Settings" button in Preferences

### Image not loading

- Only absolute paths are supported (relative paths won't work)
- Use NSImage-compatible formats: PNG, JPG, ICNS, etc.

## Building the Plugins

If you need to rebuild the native plugins:

### macOS Plugin

```bash
cd path/to/Plugins/macOS/DockIconPlugin
xcodebuild -project DockIconPlugin.xcodeproj \
  -scheme DockIconPlugin \
  -configuration Release \
  -arch x86_64 -arch arm64 \
  ONLY_ACTIVE_ARCH=NO \
  BUILD_DIR=./build \
  clean build

# Copy to package
cp -r build/Release/DockIconPlugin.bundle \
  path/to/Packages/com.mattun.unicon/Plugins/Editor/macOS/
```

### Windows Plugin

```bash
cd path/to/Plugins/Windows/DockIconPlugin
mkdir build && cd build
cmake ..
cmake --build . --config Release

# Copy to package
cp Release/DockIconPluginForWindows.dll \
  path/to/Packages/com.mattun.unicon/Plugins/Editor/Windows/
```

## License

MIT License

Copyright (c) 2025 mattun

## References

- [NSApplication.applicationIconImage - Apple Developer](https://developer.apple.com/documentation/appkit/nsapplication/1428744-applicationiconimage)
- [Unity Native Plugins](https://docs.unity3d.com/Manual/NativePlugins.html)
- [InitializeOnLoadAttribute](https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html)
- [SettingsProvider](https://docs.unity3d.com/ScriptReference/SettingsProvider.html)
