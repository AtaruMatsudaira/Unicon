using UnityEngine;

namespace Unicon
{
    internal sealed class DummyNativeMethods : INativeMethods
    {
        public bool ResetIcon()
        {
            Debug.LogWarning("Unicon: This feature is only available on macOS Editor and Windows Editor");
            return false;
        }

        public bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor, float fontSizeMultiplier)
        {
            Debug.LogWarning("Unicon: This feature is only available on macOS Editor and Windows Editor");
            return false;
        }
    }
}