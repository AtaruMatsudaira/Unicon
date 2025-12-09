using UnityEngine;

namespace Unicon
{
    internal static class NativeMethods
    {
        private static readonly INativeMethods s_impl =
#if UNITY_EDITOR_OSX
            new MacOSNativeMethods();
#elif UNITY_EDITOR_WIN
            new WindowsNativeMethods();
#else
            new DummyNativeMethods();
#endif

        public static bool ResetIcon()
        {
            return s_impl.ResetIcon();
        }

        public static bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor, float fontSizeMultiplier)
        {
            return s_impl.SetIconUnified(imagePath, overlayColor, text, textColor, fontSizeMultiplier);
        }
    }
}
