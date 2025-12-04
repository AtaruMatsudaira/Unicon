using UnityEngine;

namespace DockIconChanger
{
    internal sealed class DummyNativeMethods : INativeMethods
    {
        public bool SetIconFromPath(string imagePath)
        {
            Debug.LogWarning("DockIconChanger: This feature is only available on macOS Editor and Windows Editor");
            return false;
        }

        public bool SetIconWithColorOverlay(Color color)
        {
            Debug.LogWarning("DockIconChanger: This feature is only available on macOS Editor and Windows Editor");
            return false;
        }

        public bool ResetIcon()
        {
            Debug.LogWarning("DockIconChanger: This feature is only available on macOS Editor and Windows Editor");
            return false;
        }

        public bool SetIconWithText(string text, Color textColor)
        {
            Debug.LogWarning("DockIconChanger: This feature is only available on macOS Editor and Windows Editor");
            return false;
        }

        public bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor)
        {
            Debug.LogWarning("DockIconChanger: This feature is only available on macOS Editor and Windows Editor");
            return false;
        }
    }
}