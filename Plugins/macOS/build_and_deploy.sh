#!/bin/bash

# DockIconPlugin のビルドと配置を自動化するスクリプト
# Usage: ./build_and_deploy.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PLUGIN_DIR="$SCRIPT_DIR"
UNITY_PLUGIN_DIR="$PROJECT_ROOT/Packages/com.mattun.unicon/Plugins/Editor/macOS"
BUILD_DIR="$HOME/Library/Developer/Xcode/DerivedData"

echo "=========================================="
echo "DockIconPlugin Build & Deploy"
echo "=========================================="
echo ""

# ビルド
echo "[BUILD] Building Swift plugin (Universal Binary: arm64 + x86_64)..."
cd "$PLUGIN_DIR"
xcodebuild -project DockIconPlugin.xcodeproj \
    -scheme DockIconPlugin \
    -configuration Release \
    -arch arm64 \
    -arch x86_64 \
    ONLY_ACTIVE_ARCH=NO \
    clean build

if [ $? -ne 0 ]; then
    echo "[ERROR] Build failed!"
    exit 1
fi

echo "[SUCCESS] Build succeeded!"
echo ""

# ビルド成果物のパスを探す
BUNDLE_PATH=$(find "$BUILD_DIR" -name "DockIconPlugin.bundle" -path "*/Release/DockIconPlugin.bundle" | head -n 1)

if [ -z "$BUNDLE_PATH" ]; then
    echo "[ERROR] Could not find built bundle!"
    exit 1
fi

echo "[INFO] Found bundle at: $BUNDLE_PATH"
echo ""

# Unityプロジェクトに配置
echo "[DEPLOY] Deploying to Unity project..."
rm -rf "$UNITY_PLUGIN_DIR/DockIconPlugin.bundle"
cp -R "$BUNDLE_PATH" "$UNITY_PLUGIN_DIR/"

if [ $? -ne 0 ]; then
    echo "[ERROR] Deploy failed!"
    exit 1
fi

echo "[SUCCESS] Deploy succeeded!"
echo ""

# アーキテクチャ確認
echo "[VERIFY] Checking architectures..."
lipo -info "$UNITY_PLUGIN_DIR/DockIconPlugin.bundle/Contents/MacOS/DockIconPlugin"

# シンボル確認
echo ""
echo "[VERIFY] Verifying exported symbols..."
nm -gU "$UNITY_PLUGIN_DIR/DockIconPlugin.bundle/Contents/MacOS/DockIconPlugin" | grep SetDockIcon

echo ""
echo "=========================================="
echo "All done! Please restart Unity Editor."
echo "=========================================="
