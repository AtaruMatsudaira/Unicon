import Cocoa

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
    guard let originalIcon = NSApplication.shared.applicationIconImage else {
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
    let coloredImage = NSImage(size: size)

    coloredImage.lockFocus()
    originalIcon.draw(in: NSRect(origin: .zero, size: size))
    overlayColor.setFill()
    NSRect(origin: .zero, size: size).fill(using: .multiply)
    coloredImage.unlockFocus()

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

@_cdecl("SetDockIconWithText")
public func setDockIconWithText(_ textPointer: UnsafePointer<CChar>, _ r: Float, _ g: Float, _ b: Float, _ a: Float) {
    let text = String(cString: textPointer)
    let textColor = NSColor(calibratedRed: CGFloat(r), green: CGFloat(g), blue: CGFloat(b), alpha: CGFloat(a))

    guard let originalIcon = NSApplication.shared.applicationIconImage else {
        NSLog("DockIconPlugin: No original icon found")
        return
    }

    let iconWithText = applyTextOverlay(to: originalIcon, text: text, textColor: textColor)

    DispatchQueue.main.async {
        NSApplication.shared.applicationIconImage = iconWithText
        NSLog("DockIconPlugin: Successfully set dock icon with text: \"%@\"", text)
    }
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

    // テキスト属性
    let attributes: [NSAttributedString.Key: Any] = [
        .font: font,
        .foregroundColor: textColor,
        .strokeColor: NSColor.black,
        .strokeWidth: -2.0
    ]

    let attributedString = NSAttributedString(string: text, attributes: attributes)
    let textSize = attributedString.size()

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

        // テキストを描画
        attributedString.draw(in: textRect)

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
