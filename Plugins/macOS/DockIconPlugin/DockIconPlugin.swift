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
