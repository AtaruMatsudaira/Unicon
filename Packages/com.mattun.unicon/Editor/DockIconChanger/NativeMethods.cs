using UnityEngine;

namespace DockIconChanger
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
        
        public static bool SetIconFromPath(string imagePath)
        {
            return s_impl.SetIconFromPath(imagePath);
        }

        public static bool SetIconWithColorOverlay(Color color)
        {
            return s_impl.SetIconWithColorOverlay(color);
        }

        public static bool ResetIcon()
        {
            return s_impl.ResetIcon();
        }

        public static bool SetIconWithText(string text, Color textColor)
        {
            return s_impl.SetIconWithText(text, textColor);
        }

        public static bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor)
        {
            return s_impl.SetIconUnified(imagePath, overlayColor, text, textColor);
        }
    }
}
