import Cocoa

// Get the original Unity icon from the application bundle
private func getUnityDefaultIcon() -> NSImage? {
    // Try to get the icon from Unity's bundle
    if let appPath = Bundle.main.bundlePath as NSString?,
       let iconPath = appPath.appendingPathComponent("Contents/Resources/UnityAppIcon.icns") as String?,
       FileManager.default.fileExists(atPath: iconPath),
       let icon = NSImage(contentsOfFile: iconPath) {
        return icon
    }

    // Fallback: get from NSWorkspace (always returns non-nil)
    let icon = NSWorkspace.shared.icon(forFile: Bundle.main.bundlePath)
    return icon
}

@_cdecl("SetDockIconFromPath")
public func setDockIconFromPath(_ pathPointer: UnsafePointer<CChar>) {
    let path = String(cString: pathPointer)

    guard let image = NSImage(contentsOfFile: path) else {
        NSLog("DockIconPlugin: Failed to load image from path: %@", path)
        return
    }

    DispatchQueue.main.async {
        NSApplication.shared.applicationIconImage = image
        NSLog("DockIconPlugin: Successfully set dock icon from path: %@", path)
    }
}

@_cdecl("SetDockIconWithColorOverlay")
public func setDockIconWithColorOverlay(_ r: Float, _ g: Float, _ b: Float, _ a: Float) {
    guard let originalIcon = getUnityDefaultIcon() else {
        NSLog("DockIconPlugin: No original icon found")
        return
    }

    let overlayColor = NSColor(
        calibratedRed: CGFloat(r),
        green: CGFloat(g),
        blue: CGFloat(b),
        alpha: CGFloat(a)
    )

    let size = originalIcon.size
    let coloredImage = NSImage(size: size, flipped: false) { rect -> Bool in
        originalIcon.draw(in: NSRect(origin: .zero, size: size))
        overlayColor.setFill()
        NSRect(origin: .zero, size: size).fill(using: .multiply)
        return true
    }

    DispatchQueue.main.async {
        NSApplication.shared.applicationIconImage = coloredImage
        NSLog("DockIconPlugin: Successfully set dock icon with color overlay (r:%.2f g:%.2f b:%.2f a:%.2f)", r, g, b, a)
    }
}

@_cdecl("ResetDockIcon")
public func resetDockIcon() {
    DispatchQueue.main.async {
        NSApplication.shared.applicationIconImage = nil
        NSLog("DockIconPlugin: Successfully reset dock icon to default")
    }
}

@_cdecl("SetDockIconUnified")
public func setDockIconUnified(
    _ imagePathPointer: UnsafePointer<CChar>?,
    _ overlayR: Float, _ overlayG: Float, _ overlayB: Float, _ overlayA: Float,
    _ textPointer: UnsafePointer<CChar>?,
    _ textR: Float, _ textG: Float, _ textB: Float, _ textA: Float
) {
    // Get base image
    var baseImage: NSImage?

    if let pathPtr = imagePathPointer {
        let path = String(cString: pathPtr)
        if !path.isEmpty, let customImage = NSImage(contentsOfFile: path) {
            baseImage = customImage
            NSLog("DockIconPlugin: Using custom image from path: %@", path)
        }
    }

    if baseImage == nil {
        baseImage = getUnityDefaultIcon()
        NSLog("DockIconPlugin: Using Unity default icon")
    }

    guard let originalImage = baseImage else {
        NSLog("DockIconPlugin: Failed to get base image")
        return
    }

    // Apply color overlay if needed
    var resultImage = originalImage
    if overlayA > 0.0 {
        let overlayColor = NSColor(
            calibratedRed: CGFloat(overlayR),
            green: CGFloat(overlayG),
            blue: CGFloat(overlayB),
            alpha: CGFloat(overlayA)
        )
        resultImage = applyColorOverlay(to: resultImage, color: overlayColor)
        NSLog("DockIconPlugin: Applied color overlay (r:%.2f g:%.2f b:%.2f a:%.2f)", overlayR, overlayG, overlayB, overlayA)
    }

    // Apply text if needed
    if let txtPtr = textPointer {
        let text = String(cString: txtPtr)
        if !text.isEmpty {
            let textColor = NSColor(
                calibratedRed: CGFloat(textR),
                green: CGFloat(textG),
                blue: CGFloat(textB),
                alpha: CGFloat(textA)
            )
            resultImage = applyTextOverlay(to: resultImage, text: text, textColor: textColor)
            NSLog("DockIconPlugin: Applied text overlay: \"%@\"", text)
        }
    }

    // Set final icon
    DispatchQueue.main.async {
        NSApplication.shared.applicationIconImage = resultImage
        NSLog("DockIconPlugin: Successfully set unified dock icon")
    }
}

@_cdecl("SetDockIconWithText")
public func setDockIconWithText(_ textPointer: UnsafePointer<CChar>, _ r: Float, _ g: Float, _ b: Float, _ a: Float) {
    let text = String(cString: textPointer)
    let textColor = NSColor(calibratedRed: CGFloat(r), green: CGFloat(g), blue: CGFloat(b), alpha: CGFloat(a))

    guard let originalIcon = getUnityDefaultIcon() else {
        NSLog("DockIconPlugin: No original icon found")
        return
    }

    let iconWithText = applyTextOverlay(to: originalIcon, text: text, textColor: textColor)

    DispatchQueue.main.async {
        NSApplication.shared.applicationIconImage = iconWithText
        NSLog("DockIconPlugin: Successfully set dock icon with text: \"%@\"", text)
    }
}

private func applyColorOverlay(to image: NSImage, color: NSColor) -> NSImage {
    let size = image.size
    let coloredImage = NSImage(size: size, flipped: false) { rect -> Bool in
        image.draw(in: NSRect(origin: .zero, size: size))
        color.setFill()
        NSRect(origin: .zero, size: size).fill(using: .multiply)
        return true
    }
    return coloredImage
}

private func applyTextOverlay(to image: NSImage, text: String, textColor: NSColor) -> NSImage {
    // 正方形のサイズを決定（縦横の大きい方を使用）
    let originalSize = image.size
    let maxDimension = max(originalSize.width, originalSize.height)
    let squareSize = NSSize(width: maxDimension, height: maxDimension)

    // 元の画像を正方形にリサイズ（アスペクト比を保持して中央配置）
    let squareImage = NSImage(size: squareSize, flipped: false) { rect -> Bool in
        // 背景を透明にするため、何も描画しない

        // アスペクト比を保持して中央に配置
        let aspectWidth = originalSize.width
        let aspectHeight = originalSize.height
        let scale = min(maxDimension / aspectWidth, maxDimension / aspectHeight)
        let scaledWidth = aspectWidth * scale
        let scaledHeight = aspectHeight * scale
        let x = (maxDimension - scaledWidth) / 2
        let y = (maxDimension - scaledHeight) / 2

        let drawRect = NSRect(x: x, y: y, width: scaledWidth, height: scaledHeight)
        image.draw(in: drawRect)

        return true
    }

    let size = squareSize

    // フォントサイズを文字数に応じて調整
    let fontSize = calculateFontSize(for: text, iconSize: size.height)
    let font = NSFont.systemFont(ofSize: fontSize, weight: .bold)

    // テキスト属性（2段階描画用）
    // 1. 輪郭用（黒いstroke）
    let outlineAttributes: [NSAttributedString.Key: Any] = [
        .font: font,
        .strokeColor: NSColor.black,
        .strokeWidth: 3.0,  // 正の値でstrokeのみ描画
        .foregroundColor: NSColor.black
    ]

    // 2. 本体用（指定色）
    let fillAttributes: [NSAttributedString.Key: Any] = [
        .font: font,
        .foregroundColor: textColor
    ]

    let outlineString = NSAttributedString(string: text, attributes: outlineAttributes)
    let fillString = NSAttributedString(string: text, attributes: fillAttributes)
    let textSize = fillString.size()

    // 右下配置（5%マージン）
    let margin = size.width * 0.05
    let textRect = NSRect(
        x: size.width - textSize.width - margin,  // 右寄せ
        y: margin,  // 下寄せ
        width: textSize.width,
        height: textSize.height
    )

    // NSImage(size:flipped:drawingHandler:)を使用（lockFocus/unlockFocusは非推奨のため）
    let resultImage = NSImage(size: size, flipped: false) { rect -> Bool in
        // 正方形化した画像を描画
        squareImage.draw(in: NSRect(origin: .zero, size: size))

        // テキストを2段階で描画（まず輪郭、次に塗りつぶし）
        outlineString.draw(in: textRect)
        fillString.draw(in: textRect)

        return true
    }

    return resultImage
}

private func calculateFontSize(for text: String, iconSize: CGFloat) -> CGFloat {
    switch text.count {
    case 1:
        return iconSize * 0.4
    case 2:
        return iconSize * 0.35
    case 3:
        return iconSize * 0.28
    case 4:
        return iconSize * 0.25
    default:
        return iconSize * 0.22  // 4文字以上は最小サイズで固定
    }
}
